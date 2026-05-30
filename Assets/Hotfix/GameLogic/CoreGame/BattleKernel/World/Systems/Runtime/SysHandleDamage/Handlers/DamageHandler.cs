using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LccHotfix
{
    // 伤害计算器
    public class DamageHandler : IEntityDamageHandler
    {
        public void HandleDamage(DamageContext context, ref DamageResult result)
        {
            var world = context.World;
            var defender_id = context.Defender.FighterEnityId;
            LogicEntity defender_e = world.GetEntityWithComID(defender_id);
            if (defender_e == null)
            {
                if (BattleLog.IsDebugEnabled)
                    BattleLog.Warning($"HandleDamage defender_e == null, defender_e={defender_id}");
                return;
            }
            if (defender_e.IsDead())
            {
                return;
            }

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
                var buffList = defender_e.comBuffCenter.BuffList;
                for (int i = 0; i < buffList.Count; i++)
                {
                    var buffLogic = buffList[i] as BuffLogic;
                    buffLogic.HandleBeforeDmg(context, ref result);
                }
            }
            
            var hitInfo = context.HitInfo;
            var hitTextPos = GetHitTextPos(defender_e, hitInfo);

            //伤害丢弃 状态飘字:
            var usePrimaryStateFeedbackStyle = world.DamagePolicyService?.UsePrimaryStateFeedbackStyle(defender_e) ?? false;
            if (result.IsMiss)
            {
                world.BattleFeedbackSink?.ShowDamageMiss(hitTextPos, usePrimaryStateFeedbackStyle);
                return;
            }
            
            if (result.IsBlock)
            {
                world.BattleFeedbackSink?.ShowDamageBlock(hitTextPos, usePrimaryStateFeedbackStyle);
                return;
            }

            //伤害数字飘字:
            var damage = Mathf.FloorToInt((float)result.FinalDamage);
            defender_e.comHp.ChangeHP(-damage);
            var useTaggedDefenderStyle = world.DamagePolicyService?.UseTaggedFeedbackStyle(defender_e) ?? false;

            var displayDamage = damage + Mathf.FloorToInt((float)result.ShieldDeducted);
            if (displayDamage > 0)
            {
                world.BattleFeedbackSink?.ShowDamageNumber(displayDamage, hitTextPos, useTaggedDefenderStyle, result.IsCritical);
            }
            
            if (defender_e.comHp.Hp <= 0)
            {
                world.DamagePolicyService?.DispatchTriggerDeath(defender_e);
                var cmd = new EntityCommand() { CmdType = EntityCmdType.Nt_Death,};
                defender_e.SendCmd(cmd);
            }
            else
            {
                var cmd = new EntityCommand() { CmdType = EntityCmdType.Nt_OnHurt,};
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
