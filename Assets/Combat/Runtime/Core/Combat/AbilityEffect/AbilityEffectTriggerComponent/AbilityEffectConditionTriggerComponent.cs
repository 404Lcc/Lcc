namespace LccModel
{
    /// <summary>
    /// 条件触发组件
    /// </summary>
    public class AbilityEffectConditionTriggerComponent : Component
    {
        public override bool DefaultEnable { get; set; } = false;
        public string ConditionParamValue { get; set; }


        public override void OnEnable()
        {
            var conditionType = GetParent<AbilityEffect>().EffectConfig.ConditionType;
            var conditionParam = ConditionParamValue;
            Parent.GetParent<StatusAbility>().OwnerEntity.ListenerCondition(conditionType, OnConditionTrigger, conditionParam);
        }

        private void OnConditionTrigger()
        {
            GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }
    }
}