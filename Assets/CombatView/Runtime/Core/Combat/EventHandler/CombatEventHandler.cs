using ET;

namespace LccModel
{
    [EventHandler]
    public class SyncCreateCombatEventHandler : AEvent<SyncCreateCombat>
    {
        public override async ETTask Publish(SyncCreateCombat data)
        {
            CombatView combatView = CombatViewContext.Instance.GetCombatView(data.id);
            if (combatView == null)
            {
                CombatViewContext.Instance.AddCombatView(data.id);
            }
            await ETTask.CompletedTask;
        }
    }
    [EventHandler]
    public class SyncDeleteCombatEventHandler : AEvent<SyncDeleteCombat>
    {
        public override async ETTask Publish(SyncDeleteCombat data)
        {
            CombatViewContext.Instance.RemoveCombatView(data.id);
            await ETTask.CompletedTask;
        }
    }
    [EventHandler]
    public class SyncCreateAbilityItemEventHandler : AEvent<SyncCreateAbilityItem>
    {
        public override async ETTask Publish(SyncCreateAbilityItem data)
        {
            AbilityItemView abilityItemView = CombatViewContext.Instance.GetAbilityItemView(data.id);
            if (abilityItemView == null)
            {
                CombatViewContext.Instance.AddAbilityItemView(data.id);
            }
            await ETTask.CompletedTask;
        }
    }
    [EventHandler]
    public class SyncDeleteAbilityItemEventHandler : AEvent<SyncDeleteAbilityItem>
    {
        public override async ETTask Publish(SyncDeleteAbilityItem data)
        {
            CombatViewContext.Instance.RemoveAbilityItemView(data.id);
            await ETTask.CompletedTask;
        }
    }



    [EventHandler]
    public class SyncTransformEventHandler : AEvent<SyncTransform>
    {
        public override async ETTask Publish(SyncTransform data)
        {
            CombatView combatView = CombatViewContext.Instance.GetCombatView(data.id);
            if (combatView != null)
            {
                combatView.TransformViewComponent.SyncTransform(data.position, data.rotation, data.localScale);
            }
            AbilityItemView abilityItemView = CombatViewContext.Instance.GetAbilityItemView(data.id);
            if (abilityItemView != null)
            {
                abilityItemView.TransformViewComponent.SyncTransform(data.position, data.rotation, data.localScale);
            }
            await ETTask.CompletedTask;
        }
    }
    [EventHandler]
    public class SyncAnimationEventHandler : AEvent<SyncAnimation>
    {
        public override async ETTask Publish(SyncAnimation data)
        {
            CombatView combatView = CombatViewContext.Instance.GetCombatView(data.id);
            if (combatView != null)
            {
                combatView.AnimationViewComponent.PlayAnimation(data.type, data.speed, data.isLoop);
            }
            await ETTask.CompletedTask;
        }
    }

    [EventHandler]
    public class SyncDeadEventHandler : AEvent<SyncDead>
    {
        public override async ETTask Publish(SyncDead data)
        {

            await ETTask.CompletedTask;
        }
    }
    [EventHandler]
    public class SyncDamageEventHandler : AEvent<SyncDamage>
    {
        public override async ETTask Publish(SyncDamage data)
        {

            await ETTask.CompletedTask;
        }
    }
    [EventHandler]
    public class SyncCureEventHandler : AEvent<SyncCure>
    {
        public override async ETTask Publish(SyncCure data)
        {

            await ETTask.CompletedTask;
        }
    }
}