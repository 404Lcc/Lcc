namespace LccModel
{
    /// <summary>
    /// 行动点触发组件
    /// </summary>
    public class AbilityEffectActionTriggerComponent : Component
    {
        public override bool DefaultEnable => false;


        public override void OnEnable()
        {
            var actionPointType = GetParent<AbilityEffect>().effectConfig.ActionPointType;
            GetParent<AbilityEffect>().GetParent<StatusAbility>().OwnerEntity.ListenActionPoint(actionPointType, OnActionPointTrigger);
        }

        private void OnActionPointTrigger(Entity combatAction)
        {
            GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            var actionPointType = GetParent<AbilityEffect>().effectConfig.ActionPointType;
            GetParent<AbilityEffect>().GetParent<StatusAbility>().OwnerEntity.UnListenActionPoint(actionPointType, OnActionPointTrigger);
        }
    }
}