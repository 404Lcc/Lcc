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
                BattleLog.Warning($"HandleHeal target == null, targetId={targetId}");
                return;
            }

            if (!target.hasComHp)
            {
                BattleLog.Warning($"HandleHeal target do not has comHp, targetId={targetId}");
                return;
            }
            //BattleLog.Debug($"HandleDamage entity={entity.ID}, FinalDamage={result.FinalDamage}");

            var curHp = target.comHp.Hp;
            var maxHp = target.comHp.MaxHp;
            var pos = target.comTransform.position;
            var healing = context.Healing;
            if (healing + curHp >= maxHp)
                healing = (float)maxHp - (float)curHp;
            if (healing <= 0)
                return;
            healing = Mathf.RoundToInt(healing);
            target.comHp.ChangeHP(healing);
            var hitBindPointPos = pos;
            if(target.hasComView) 
                hitBindPointPos = target.GetMainViewBindPos("Hit");
            var useTaggedTargetStyle = world.DamagePolicyService?.UseTaggedFeedbackStyle(target) ?? false;
            world.BattleFeedbackSink?.ShowHealNumber(Mathf.RoundToInt(healing), hitBindPointPos, useTaggedTargetStyle);
        }
    }
}
