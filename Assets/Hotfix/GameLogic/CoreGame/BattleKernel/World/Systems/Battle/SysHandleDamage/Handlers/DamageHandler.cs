using UnityEngine;
using Random = UnityEngine.Random;

namespace LccHotfix
{
    // 伤害计算器（通用结算）；玩法特化经 IDamagePolicyService 注入。
    public class DamageHandler : IEntityDamageHandler
    {
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

            if (defender_e.hasComBuffCenter)
            {
                defender_e.comBuffCenter.HandleBeforeDmg(context, ref result);
            }

            var creationInfo = world.GetCreationInfo<BattleKernelCreationInfo>();
            var damagePolicy = creationInfo.DamagePolicyService;
            damagePolicy?.ModifyDamageResult(context, defender_e, ref result);

            var hitInfo = context.HitInfo;
            var hitTextPos = GetHitTextPos(defender_e, hitInfo);

            if (result.IsMiss)
            {
                creationInfo.BattleFeedbackSink?.ShowDamageMiss(hitTextPos);
                return;
            }

            if (result.IsBlock)
            {
                creationInfo.BattleFeedbackSink?.ShowDamageBlock(hitTextPos);
                return;
            }

            if (result.IsImmune)
            {
                creationInfo.BattleFeedbackSink?.ShowDamageImmune(hitTextPos);
                return;
            }

            if (double.IsNaN(result.FinalDamage) || double.IsInfinity(result.FinalDamage))
            {
                BattleLogger.LogError($"伤害结算异常，已阻止扣血：攻击实体={context.Attacker.FighterEnityId}，目标实体={defender_e.ID}，最终伤害={result.FinalDamage}");
                return;
            }

            var damage = Mathf.FloorToInt((float)result.FinalDamage);
            defender_e.comHp.ChangeHP(-damage);
            damagePolicy?.OnAfterDamageApplied(context, defender_e, result, damage);

            var displayDamage = damage + Mathf.FloorToInt((float)result.ShieldDeducted);
            if (displayDamage > 0)
            {
                var isPoisonDot = damagePolicy?.IsPoisonDot(context) ?? false;
                creationInfo.BattleFeedbackSink?.ShowDamageNumber(displayDamage, hitTextPos, result.IsCritical, isPoisonDot);
            }

            if (defender_e.comHp.Hp > 0)
            {
                var isMeleeHit = damagePolicy?.IsMeleeHit(context) ?? false;
                var cmd = new EntityCommand()
                {
                    CmdType = EntityCmdType.Nt_OnHurt,
                    V0 = isMeleeHit,
                };
                defender_e.SendCmd(cmd);
            }
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
