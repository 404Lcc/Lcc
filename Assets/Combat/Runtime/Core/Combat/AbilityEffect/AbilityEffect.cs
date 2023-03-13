using System;
using System.Collections.Generic;

namespace LccModel
{
    public partial class AbilityEffect : Entity
    {
        public Effect effect;

        public Entity OwnerAbility => (Entity)Parent; //AbilityEffect是挂在能力上的
        public Combat Owner => ((IAbility)OwnerAbility).Owner;
        
        
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

                if (effect.EffectTriggerType == EffectTriggerType.Interval && !string.IsNullOrEmpty(effect.IntervalValueFormula))
                {
                    AddComponent<AbilityEffectIntervalTriggerComponent>();
                }

                if (effect.EffectTriggerType == EffectTriggerType.Condition && !string.IsNullOrEmpty(effect.ConditionValueFormula))
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
                temp.Add("自身生命值", Owner.GetComponent<AttributeComponent>().HealthPoint.Value.ToString());
                temp.Add("自身攻击力", Owner.GetComponent<AttributeComponent>().Attack.Value.ToString());
            }
            return temp;
        }

        public void TryAssignEffectToOwner()
        {
            TryAssignEffectToTarget(Owner);
        }

        public void TryAssignEffectToTarget(Combat target)
        {
            if (Owner.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = target;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.ApplyEffectAssign();
            }
        }

        public void TryAssignEffectToTarget(Combat target, IActionExecution actionExecution)
        {
            if (Owner.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = target;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.actionExecution = actionExecution;
                action.ApplyEffectAssign();
            }
        }

        public void TryAssignEffectToTarget(Combat target, IAbilityExecution abilityExecution)
        {
            if (Owner.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = target;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.abilityExecution = abilityExecution;
                action.ApplyEffectAssign();
            }
        }

        public void TryAssignEffectToTarget(Combat target, AbilityItem abilityItem)
        {
            if (Owner.effectAssignActionAbility.TryMakeAction(out var action))
            {
                action.Target = target;
                action.sourceAbility = OwnerAbility;
                action.abilityEffect = this;
                action.abilityItem = abilityItem;
                action.ApplyEffectAssign();
            }
        }

        public void StartAssignEffect(EffectAssignAction action)
        {
            if (effect is AddStatusEffect)
            {
                GetComponent<AbilityEffectAddStatusComponent>().OnAssignEffect(action);
            }

            if (effect is ClearAllStatusEffect)
            {

            }

            if (effect is CureEffect)
            {
                GetComponent<AbilityEffectCureComponent>().OnAssignEffect(action);
            }

            if (effect is DamageEffect)
            {
                GetComponent<AbilityEffectDamageComponent>().OnAssignEffect(action);
            }

            if (effect is RemoveStatusEffect)
            {

            }
        }
    }
}