using System.Collections.Generic;

namespace LccModel
{
    public partial class AbilityEffect : Entity
    {
        public Effect effect;

        public Entity OwnerAbility => (Entity)Parent; //AbilityEffect是挂在能力上的
        public CombatEntity OwnerEntity => ((IAbilityEntity)OwnerAbility).OwnerEntity;
        public CombatEntity ParentEntity => ((IAbilityEntity) OwnerAbility).ParentEntity;


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            effect = p1 as Effect;

            if (effect is AddStatusEffect)
            {
                AddComponent<AbilityEffectAddStatusComponent>();
            }

            if (effect is ClearAllStatusEffect)
            {

            }

            if (effect is CureEffect)
            {
                AddComponent<AbilityEffectCureComponent>();
            }

            if (effect is DamageEffect)
            {
                AddComponent<AbilityEffectDamageComponent>();
            }

            if (effect is RemoveStatusEffect)
            {

            }

            AddComponent<AbilityEffectDecoratosComponent>();
        }

        public void EnableEffect()
        {
            if (effect is ActionControlEffect)
            {
                AddComponent<AbilityEffectActionControlComponent>();
            }
            if (effect is AttributeModifyEffect)
            {
                AddComponent<AbilityEffectAttributeModifyComponent>();
            }
            if (effect is CustomEffect)
            {
                AddComponent<AbilityEffectCustomComponent>();
            }
            if (effect is DamageBloodSuckEffect)
            {
                AddComponent<AbilityEffectDamageBloodSuckComponent>();
            }

            if (effect is not ActionControlEffect && effect is not AttributeModifyEffect)
            {
                if (effect.EffectTriggerType == EffectTriggerType.Instant)
                {
                    TryAssignEffectToOwner();
                }

                if (effect.EffectTriggerType == EffectTriggerType.Action)
                {
                    AddComponent<AbilityEffectActionTriggerComponent>();
                }

                if (effect.EffectTriggerType == EffectTriggerType.Interval && !string.IsNullOrEmpty(effect.Interval))
                {
                    AddComponent<AbilityEffectIntervalTriggerComponent>();
                }

                if (effect.EffectTriggerType == EffectTriggerType.Condition && !string.IsNullOrEmpty(effect.ConditionParams))
                {
                    AddComponent<AbilityEffectConditionTriggerComponent>();
                }
            }
        }

        public Dictionary<string, string> GetParamsDict()
        {
            Dictionary<string, string> temp;
            if (OwnerAbility is StatusAbility status)
            {
                temp = status.paramsDict;
                return temp;
            }
            else
            {
                temp = new Dictionary<string, string>();
                temp.Add("自身生命值", OwnerEntity.GetComponent<AttributeComponent>().HealthPoint.Value.ToString());
                temp.Add("自身攻击力", OwnerEntity.GetComponent<AttributeComponent>().Attack.Value.ToString());
            }
            return temp;
        }

        public void TryAssignEffectToOwner()
        {
            TryAssignEffectToTarget(OwnerEntity);
        }

        public void TryAssignEffectToParent()
        {
            TryAssignEffectToTarget(ParentEntity);
        }

        public void TryAssignEffectToTarget(CombatEntity targetEntity)
        {
            if (OwnerEntity.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.ApplyEffectAssign();
            }
        }

        public void TryAssignEffectToTarget(CombatEntity targetEntity, IActionExecution actionExecution)
        {
            if (OwnerEntity.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.actionExecution = actionExecution;
                action.ApplyEffectAssign();
            }
        }

        public void TryAssignEffectToTarget(CombatEntity targetEntity, IAbilityExecution abilityExecution)
        {
            if (OwnerEntity.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.abilityExecution = abilityExecution;
                action.ApplyEffectAssign();
            }
        }

        public void TryAssignEffectToTarget(CombatEntity targetEntity, AbilityItem abilityItem)
        {
            if (OwnerEntity.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.abilityItem = abilityItem;
                action.ApplyEffectAssign();
            }
        }

        public void StartAssignEffect(EffectAssignAction action)
        {
            FireEvent(nameof(StartAssignEffect), action);
        }
    }
}