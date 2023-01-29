using NPOI.SS.UserModel;

namespace LccModel
{
    /// <summary>
    /// 条件触发组件
    /// </summary>
    public class AbilityEffectConditionTriggerComponent : Component
    {
        public override bool DefaultEnable => false;
        public string conditionValueFormula;


        public override void OnEnable()
        {
            var conditionType = GetParent<AbilityEffect>().effectConfig.ConditionType;
            var conditionParam = conditionValueFormula;
            Parent.GetParent<StatusAbility>().OwnerEntity.ListenerCondition(conditionType, OnConditionTrigger, conditionParam);
        }

        private void OnConditionTrigger()
        {
            this.GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            var conditionType = GetParent<AbilityEffect>().effectConfig.ConditionType;
            Parent.GetParent<StatusAbility>().OwnerEntity.UnListenCondition(conditionType, OnConditionTrigger);
        }
    }
}