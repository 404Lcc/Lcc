using UnityEngine;

namespace LccHotfix
{
    // 伤害计算器
    public class HealHandler : IEntityHealHandler
    {
        public void HandleHeal(HealContext context)
        {
            var world = context.World;
            var targetId = context.Target.FighterEnityId;
            LogicEntity target = world.GetEntityWithComID(targetId);
            if (target == null)
            {
                BattleLogger.LogWarning($"HandleHeal target == null, targetId={targetId}");
                return;
            }

            if (!target.hasComHp)
            {
                BattleLogger.LogWarning($"HandleHeal target do not has comHp, targetId={targetId}");
                return;
            }
            //BattleLog.Debug($"HandleDamage entity={entity.ID}, FinalDamage={result.FinalDamage}");

            var curHp = target.comHp.Hp;
            var maxHp = target.comHp.MaxHp;
            var healing = context.Healing;
            if (curHp >= maxHp || healing <= 0)
                return;

            var missingHp = (float)(maxHp - curHp);
            if (healing > missingHp)
                healing = missingHp;
            var healAmount = Mathf.RoundToInt(healing);
            if (healAmount <= 0)
                return;

            target.comHp.ChangeHP(healAmount);
            var hitBindPointPos = target.comTransform.position;
            if (target.hasComView)
                hitBindPointPos = target.GetMainViewBindPos("Hit");

            world.GetCreationInfo<BattleKernelCreationInfo>().BattleFeedbackSink?.ShowHealNumber(healAmount, hitBindPointPos);
        }
    }
}
