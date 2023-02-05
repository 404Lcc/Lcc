namespace LccModel
{
    /// <summary>
    /// 条件触发组件
    /// </summary>
    public class AbilityEffectConditionTriggerComponent : Component
    {

        public string conditionValueFormula;


        public override void Awake()
        {
            base.Awake();

            var conditionType = GetParent<AbilityEffect>().effectConfig.ConditionType;
            var conditionParam = conditionValueFormula;
            Parent.GetParent<StatusAbility>().OwnerEntity.ListenerCondition(conditionType, OnConditionTrigger, conditionParam);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            var conditionType = GetParent<AbilityEffect>().effectConfig.ConditionType;
            Parent.GetParent<StatusAbility>().OwnerEntity.UnListenCondition(conditionType, OnConditionTrigger);
        }

        private void OnConditionTrigger()
        {
            this.GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }


    }
}