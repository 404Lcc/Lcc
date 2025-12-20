using ET;

namespace LccModel
{
    [Event]
    public class SyncCreateCombatEventHandler : AEvent<SyncCreateCombat>
    {
        protected override async ETTask Run(SyncCreateCombat data)
        {
            CombatView combatView = CombatViewContext.Instance.GetCombatView(data.id);
            if (combatView == null)
            {
                CombatViewContext.Instance.AddCombatView(data.id);
            }
            await ETTask.CompletedTask;
        }
    }
    [Event]
    public class SyncDeleteCombatEventHandler : AEvent<SyncDeleteCombat>
    {
        protected override async ETTask Run(SyncDeleteCombat data)
        {
            CombatViewContext.Instance.RemoveCombatView(data.id);
            await ETTask.CompletedTask;
        }
    }
    [Event]
    public class SyncCreateAbilityItemEventHandler : AEvent<SyncCreateAbilityItem>
    {
        protected override async ETTask Run(SyncCreateAbilityItem data)
        {
            AbilityItemView abilityItemView = CombatViewContext.Instance.GetAbilityItemView(data.id);
            if (abilityItemView == null)
            {
                CombatViewContext.Instance.AddAbilityItemView(data.id);
            }
            await ETTask.CompletedTask;
        }
    }
    [Event]
    public class SyncDeleteAbilityItemEventHandler : AEvent<SyncDeleteAbilityItem>
    {
        protected override async ETTask Run(SyncDeleteAbilityItem data)
        {
            CombatViewContext.Instance.RemoveAbilityItemView(data.id);
            await ETTask.CompletedTask;
        }
    }





    [Event]
    public class SyncTransformEventHandler : AEvent<SyncTransform>
    {
        protected override async ETTask Run(SyncTransform data)
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
    [Event]
    public class SyncAnimationEventHandler : AEvent<SyncAnimation>
    {
        protected override async ETTask Run(SyncAnimation data)
        {
            CombatView combatView = CombatViewContext.Instance.GetCombatView(data.id);
            if (combatView != null)
            {
                combatView.AnimationViewComponent.PlayAnimation(data.type, data.speed, data.isLoop);
            }
            await ETTask.CompletedTask;
        }
    }




    [Event]
    public class SyncParticleEffectEventHandler : AEvent<SyncParticleEffect>
    {
        protected override async ETTask Run(SyncParticleEffect data)
        {
            await ETTask.CompletedTask;
        }
    }
    [Event]
    public class SyncDeleteParticleEffectEventHandler : AEvent<SyncDeleteParticleEffect>
    {
        protected override async ETTask Run(SyncDeleteParticleEffect data)
        {

            await ETTask.CompletedTask;
        }
    }




    [Event]
    public class SyncDamageEventHandler : AEvent<SyncDamage>
    {
        protected override async ETTask Run(SyncDamage data)
        {
            CombatView combatView = CombatViewContext.Instance.GetCombatView(data.id);
            if (combatView != null)
            {
                LogUtil.Debug(data.id + "受到伤害" + data.damage);
                LogUtil.Debug(data.id + "剩余血量" + combatView.AttributeViewComponent.HealthPoint.Value);
            }

            await ETTask.CompletedTask;
        }
    }
    [Event]
    public class SyncCureEventHandler : AEvent<SyncCure>
    {
        protected override async ETTask Run(SyncCure data)
        {
            CombatView combatView = CombatViewContext.Instance.GetCombatView(data.id);
            if (combatView != null)
            {
                LogUtil.Debug(data.id + "受到治疗" + data.cure);
                LogUtil.Debug(data.id + "剩余血量" + combatView.AttributeViewComponent.HealthPoint.Value);
            }

            await ETTask.CompletedTask;
        }
    }



    [Event]
    public class SyncAttributeEventHandler : AEvent<SyncAttribute>
    {
        protected override async ETTask Run(SyncAttribute data)
        {
            CombatView combatView = CombatViewContext.Instance.GetCombatView(data.id);
            if (combatView != null)
            {
                combatView.AttributeViewComponent.AddNumeric(data.attributeType);
            }
            await ETTask.CompletedTask;
        }
    }
    [Event]
    public class SyncModifyAttributeEventHandler : AEvent<SyncModifyAttribute>
    {
        protected override async ETTask Run(SyncModifyAttribute data)
        {
            CombatView combatView = CombatViewContext.Instance.GetCombatView(data.id);
            if (combatView != null)
            {
                combatView.AttributeViewComponent.GetNumeric((AttributeType)data.type).ModifyAttribute(data.temp, data.value);
            }
            await ETTask.CompletedTask;
        }
    }
}