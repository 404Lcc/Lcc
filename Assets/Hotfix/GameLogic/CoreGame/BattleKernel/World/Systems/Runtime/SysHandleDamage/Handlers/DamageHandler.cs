using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LccHotfix
{
    // 伤害计算器
    public class DamageHandler : IEntityDamageHandler
    {
        private const uint TacticalCharmAttackDroneBulletSubobjectTid = 2245;
        private const string TacticalCharmAttackDroneLogTag = "攻击无人机测试log";

        public void HandleDamage(DamageContext context, ref DamageResult result)
        {
            var world = context.World;
            var defender_id = context.Defender.FighterEnityId;
            LogicEntity defender_e = world.GetEntityWithComID(defender_id);
            if (defender_e == null)
            {
                if (BattleLogger.IsDebugEnabled)
                    BattleLogger.LogWarning($"HandleDamage defender_e == null, defender_e={defender_id}");
                return;
            }
            if (defender_e.IsDead())
            {
                return;
            }

            // 无论后续处理如何，受击事件
            var hitCmd = new EntityCommand()
            {
                CmdType = EntityCmdType.Nt_OnHit
            };
            defender_e.SendCmd(hitCmd);

            if (defender_e.hasComAttributes)
            {
                var canBeHurted = defender_e.comAttributes.GetAttribute<bool>(AttributeBool.CanBeHurted);
                if (!canBeHurted.Value)
                {
                    return;
                }
            }

            //BattleLog.Debug($"HandleDamage entity={entity.ID}, FinalDamage={result.FinalDamage}");

            if (defender_e.hasComBuffCenter)
            {
                defender_e.comBuffCenter.HandleBeforeDmg(context, ref result);
            }

            world.GetCreationInfo<BattleKernelCreationInfo>().DamagePolicyService?.ModifyDamageResult(context, defender_e, ref result);
            
            var hitInfo = context.HitInfo;
            var hitTextPos = GetHitTextPos(defender_e, hitInfo);

            //伤害丢弃 状态飘字:
            
            if (result.IsMiss)
            {
                world.GetCreationInfo<BattleKernelCreationInfo>().BattleFeedbackSink?.ShowDamageMiss(hitTextPos);
                return;
            }
            
            if (result.IsBlock)
            {
                world.GetCreationInfo<BattleKernelCreationInfo>().BattleFeedbackSink?.ShowDamageBlock(hitTextPos);
                return;
            }

            if (result.IsImmune)
            {
                world.GetCreationInfo<BattleKernelCreationInfo>().BattleFeedbackSink?.ShowDamageImmune(hitTextPos);
                return;
            }

            if (double.IsNaN(result.FinalDamage) || double.IsInfinity(result.FinalDamage))
            {
                BattleLogger.LogError($"伤害结算异常，已阻止扣血：攻击实体={context.Attacker.FighterEnityId}，目标实体={defender_e.ID}，最终伤害={result.FinalDamage}");
                return;
            }

            //伤害数字飘字:
            var hpBeforeDamage = defender_e.comHp.Hp;
            var damage = Mathf.FloorToInt((float)result.FinalDamage);
            defender_e.comHp.ChangeHP(-damage);

            var displayDamage = damage + Mathf.FloorToInt((float)result.ShieldDeducted);
            if (displayDamage > 0)
            {
                world.GetCreationInfo<BattleKernelCreationInfo>().BattleFeedbackSink?.ShowDamageNumber(displayDamage, hitTextPos, result.IsCritical, false);
            }

            // if (KLogger.IsDev)
            //     LogTacticalCharmAttackDroneDamage(context, result, defender_e, hpBeforeDamage, damage, displayDamage);

            if (defender_e.comHp.Hp > 0)
            {
                var cmd = new EntityCommand()
                {
                    CmdType = EntityCmdType.Nt_OnHurt,
                    V0 = false,
                };
                defender_e.SendCmd(cmd);
            }

        }

        private static void LogTacticalCharmAttackDroneDamage(
            DamageContext context,
            DamageResult result,
            LogicEntity defender,
            double hpBeforeDamage,
            int damage,
            int displayDamage)
        {
            if (context.Subobject.GetValueOrDefault().SubobjectTid != TacticalCharmAttackDroneBulletSubobjectTid)
            {
                return;
            }

            var attackerProperties = context.Attacker.Properties;
            var defenderProperties = context.Defender.Properties;
            var attackScale = Mathf.Max(0f, 1f + (float)attackerProperties.ScaleAtk / 10000f);
            var attack = attackerProperties.Attack * attackScale;
            var defenseBeforePierce = defenderProperties.Defense * (1f + (float)defenderProperties.ScaleDef / 10000f);
            var armorPierce = Mathf.Clamp01((float)attackerProperties.ArmorPierce / 10000f);
            var defense = defenseBeforePierce * (1f - armorPierce);
            var attackDefensePart = attack + defense > 0d ? attack * attack / (attack + defense) : 0d;
            var skillPart = attackDefensePart * context.SkillDamageFactor + context.SkillFixedDamage;
            var outgameFactor = Math.Max(0.2d, 1d + attackerProperties.DamageAmplify / 10000d - defenderProperties.DamageResistance / 10000d);
            var ingameFactor = Math.Max(0.2d, 1d + attackerProperties.InGameAmplify / 10000d - defenderProperties.InGameResistance / 10000d);
            var stageFactor = 1d + context.StageDamageFactor;
            var baseDamage = Math.Max(0d, skillPart * outgameFactor * ingameFactor * stageFactor + context.FinalFixedDamage);
            var typedDamage = context.World?.GetCreationInfo<BattleKernelCreationInfo>()?.DamagePropertyModifier?.ApplyDamageTypeDamage(context, baseDamage) ?? baseDamage;
            var criticalFactor = result.IsCritical ? 1d + attackerProperties.CritDamage / 10000d : 1d;
            var criticalDamage = typedDamage * criticalFactor + (result.IsCritical ? context.ExtraCritDamage : 0d);
            var randomFactor = 1d + context.RandomFinalDamageRate + attackerProperties.FinalAtk / 10000d;
            var formulaDamage = criticalDamage * randomFactor;

            BattleLogger.LogWarning(
                $"{TacticalCharmAttackDroneLogTag} 伤害结算：队长实体={context.Attacker.FighterEnityId}，目标实体={defender.ID}，" +
                $"队长攻击={attackerProperties.Attack:F2}*攻击倍率{attackScale:F4}={attack:F2}，" +
                $"目标防御={defenderProperties.Defense:F2}*防御倍率{1f + (float)defenderProperties.ScaleDef / 10000f:F4}={defenseBeforePierce:F2}，" +
                $"破甲={armorPierce:P2}后防御={defense:F2}，攻防基数={attackDefensePart:F2}，" +
                $"伤害系数={context.SkillDamageFactor:F4}，技能固定值={context.SkillFixedDamage:F2}，技能段={skillPart:F2}，" +
                $"局外倍率={outgameFactor:F4}，局内倍率={ingameFactor:F4}，关卡倍率={stageFactor:F4}，最终固定值={context.FinalFixedDamage:F2}，基础伤害={baseDamage:F2}，元素修正后={typedDamage:F2}，" +
                $"暴击倍率={criticalFactor:F4}，暴击额外值={context.ExtraCritDamage:F2}，随机最终倍率={randomFactor:F4}，" +
                $"公式结果={formulaDamage:F2}，结算最终伤害={result.FinalDamage:F2}，实际扣血={damage}，护盾扣除={result.ShieldDeducted:F2}，飘字数值={displayDamage}，" +
                $"目标生命={hpBeforeDamage:F2}->{defender.comHp.Hp:F2}，暴击={result.IsCritical}，秒杀={result.IsInstantKill}");
        }

        private static Vector3 GetHitTextPos(LogicEntity defender_e, HitInfo? hitInfo)
        {
            var pos = defender_e.comTransform.position;
            var posOffsetX = Random.Range(-0.5f, 0.5f);
            var posOffsetY = Random.Range(-0.5f, 0.5f);
            var offset = new Vector3(posOffsetX, posOffsetY, 0f);
            var hitPos = hitInfo?.hitPos;
            var hitBindPointPos = hitPos ?? pos;
            if (defender_e.hasComView)
                hitBindPointPos = defender_e.GetMainViewBindPos("Hit");
            Vector3 hitTextPos = hitBindPointPos + offset;
            return hitTextPos;
        }
    }
}
