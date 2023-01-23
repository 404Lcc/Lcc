using System.Collections.Generic;

namespace LccModel
{
    public partial class StatusAbility : Entity, IAbilityEntity
    {
#if !EGAMEPLAY_EXCEL
        /// 投放者、施术者
        public CombatEntity OwnerEntity { get; set; }
        public CombatEntity ParentEntity => GetParent<CombatEntity>();
        public bool Enable { get; set; }
        public StatusConfigObject StatusConfig { get; set; }
        //public ActionControlType ActionControlType { get; set; }
        //public Dictionary<string, FloatModifier> AddModifiers { get; set; } = new Dictionary<string, FloatModifier>();
        //public Dictionary<string, FloatModifier> PctAddModifiers { get; set; } = new Dictionary<string, FloatModifier>();
        public bool IsChildStatus { get; set; }
        public int Duration { get; set; }
        public ChildStatus ChildStatusData { get; set; }
        private List<StatusAbility> ChildrenStatuses { get; set; } = new List<StatusAbility>();


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            StatusConfig = p1 as StatusConfigObject;
            //Name = StatusConfig.ID;

            // 逻辑触发
            if (StatusConfig.Effects.Count > 0)
            {
                AddComponent<AbilityEffectComponent>(StatusConfig.Effects);
            }
        }

        /// 激活
        public void ActivateAbility()
        {
            FireEvent(nameof(ActivateAbility), this);

            /// 子状态效果
            if (StatusConfig.EnableChildrenStatuses)
            {
                foreach (var childStatusData in StatusConfig.ChildrenStatuses)
                {
                    var status = ParentEntity.AttachStatus(childStatusData.StatusConfigObject);
                    status.OwnerEntity = OwnerEntity;
                    status.IsChildStatus = true;
                    status.ChildStatusData = childStatusData;
                    status.ProcessInputKVParams(childStatusData.Params);
                    status.TryActivateAbility();
                    ChildrenStatuses.Add(status);
                }
            }

            Enable = true;
            GetComponent<AbilityEffectComponent>().Enable = true;
        }

        /// 结束
        public void EndAbility()
        {
            /// 子状态效果
            if (StatusConfig.EnableChildrenStatuses)
            {
                foreach (var item in ChildrenStatuses)
                {
                    item.EndAbility();
                }
                ChildrenStatuses.Clear();
            }

            foreach (var effect in StatusConfig.Effects)
            {
                if (!effect.Enabled)
                {
                    continue;
                }
            }

            ParentEntity.OnStatusRemove(this);

            Dispose();
        }

        public int GetDuration()
        {
            return Duration;
        }

#endif
        public Entity CreateExecution()
        {
            return null;
        }

        public void TryActivateAbility()
        {
            this.ActivateAbility();
        }

        public override void OnDestroy()
        {
            DeactivateAbility();
        }

        public void DeactivateAbility()
        {
            Enable = false;
        }

        /// 这里处理技能传入的参数数值替换
        public void ProcessInputKVParams(Dictionary<string, string> Params)
        {
            foreach (var abilityEffect in GetComponent<AbilityEffectComponent>().AbilityEffects)
            {
                var effect = abilityEffect.EffectConfig;

                if (abilityEffect.TryGetComponent(out EffectIntervalTriggerComponent intervalTriggerComponent))
                {
                    intervalTriggerComponent.IntervalValue = ProcessReplaceKV(effect.Interval, Params);
                }
                if (abilityEffect.TryGetComponent(out EffectConditionTriggerComponent conditionTriggerComponent))
                {
                    conditionTriggerComponent.ConditionParamValue = ProcessReplaceKV(effect.ConditionParam, Params);
                }

                if (effect is AttributeModifyEffect attributeModify && abilityEffect.TryGetComponent(out EffectAttributeModifyComponent attributeModifyComponent))
                {
                    attributeModifyComponent.ModifyValueFormula = ProcessReplaceKV(attributeModify.NumericValue, Params);
                }
                if (effect is DamageEffect damage && abilityEffect.TryGetComponent(out EffectDamageComponent damageComponent))
                {
                    damageComponent.DamageValueFormula = ProcessReplaceKV(damage.DamageValueFormula, Params);
                }
                if (effect is CureEffect cure && abilityEffect.TryGetComponent(out EffectCureComponent cureComponent))
                {
                    cureComponent.CureValueProperty = ProcessReplaceKV(cure.CureValueFormula, Params);
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