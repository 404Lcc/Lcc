using System.Collections.Generic;

namespace LccModel
{
    public partial class StatusAbility : Entity, IAbilityEntity
    {
        public CombatEntity OwnerEntity { get; set; }
        public CombatEntity ParentEntity => GetParent<CombatEntity>();
        public bool Enable { get; set; }


        public StatusConfigObject statusConfig;

        public bool isChildStatus;
        public int duration;
        public ChildStatus childStatusData;
        private List<StatusAbility> _statusList = new List<StatusAbility>();


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            statusConfig = p1 as StatusConfigObject;

            if (statusConfig.EffectList.Count > 0)
            {
                AddComponent<AbilityEffectComponent, List<Effect>>(statusConfig.EffectList);
            }
        }


        public void ActivateAbility()
        {
            Enable = true;
            GetComponent<AbilityEffectComponent>().EnableEffect();
            if (statusConfig.EnableChildStatus)
            {
                foreach (var childStatusData in statusConfig.StatusList)
                {
                    var status = ParentEntity.AttachStatus(childStatusData.StatusConfigObject);
                    status.OwnerEntity = OwnerEntity;
                    status.isChildStatus = true;
                    status.childStatusData = childStatusData;
                    status.ProcessInputKVParams(childStatusData.ParamsDict);
                    status.ActivateAbility();
                    _statusList.Add(status);
                }
            }
        }


        public void EndAbility()
        {
            Enable = false;
            if (statusConfig.EnableChildStatus)
            {
                foreach (var item in _statusList)
                {
                    item.EndAbility();
                }
                _statusList.Clear();
            }

            foreach (var effect in statusConfig.EffectList)
            {
                if (!effect.Enabled)
                {
                    continue;
                }
            }

            ParentEntity.OnStatusRemove(this);

            Dispose();
        }


        public Entity CreateExecution()
        {
            return null;
        }



        public void ProcessInputKVParams(Dictionary<string, string> Params)
        {
            foreach (var abilityEffect in GetComponent<AbilityEffectComponent>().abilityEffectList)
            {
                var effect = abilityEffect.effectConfig;

                if (abilityEffect.TryGetComponent(out AbilityEffectIntervalTriggerComponent intervalTriggerComponent))
                {
                    intervalTriggerComponent.intervalValueFormula = ProcessReplaceKV(effect.Interval, Params);
                }
                if (abilityEffect.TryGetComponent(out AbilityEffectConditionTriggerComponent conditionTriggerComponent))
                {
                    conditionTriggerComponent.conditionValueFormula = ProcessReplaceKV(effect.ConditionParams, Params);
                }

                if (effect is AttributeModifyEffect attributeModify && abilityEffect.TryGetComponent(out AbilityEffectAttributeModifyComponent attributeModifyComponent))
                {
                    attributeModifyComponent.numericValueFormula = ProcessReplaceKV(attributeModify.NumericValueFormula, Params);
                }
                if (effect is DamageEffect damage && abilityEffect.TryGetComponent(out AbilityEffectDamageComponent damageComponent))
                {
                    damageComponent.damageValueFormula = ProcessReplaceKV(damage.DamageValueFormula, Params);
                }
                if (effect is CureEffect cure && abilityEffect.TryGetComponent(out AbilityEffectCureComponent cureComponent))
                {
                    cureComponent.cureValueFormula = ProcessReplaceKV(cure.CureValueFormula, Params);
                }
            }
        }

        private string ProcessReplaceKV(string originValue, Dictionary<string, string> Params)
        {
            foreach (var aInputKVItem in Params)
            {
                if (!string.IsNullOrEmpty(originValue))
                {
                    originValue = originValue.Replace(aInputKVItem.Key, aInputKVItem.Value);
                }
            }
            return originValue;
        }
    }
}