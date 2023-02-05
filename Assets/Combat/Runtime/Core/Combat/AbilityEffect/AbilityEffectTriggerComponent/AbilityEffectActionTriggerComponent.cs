namespace LccModel
{
    /// <summary>
    /// 行动点触发组件
    /// </summary>
    public class AbilityEffectActionTriggerComponent : Component
    {

        public override void Awake()
        {
            base.Awake();
            var actionPointType = GetParent<AbilityEffect>().effectConfig.ActionPointType;
            GetParent<AbilityEffect>().GetParent<StatusAbility>().OwnerEntity.ListenActionPoint(actionPointType, OnActionPointTrigger);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();


            var actionPointType = GetParent<AbilityEffect>().effectConfig.ActionPointType;
            GetParent<AbilityEffect>().GetParent<StatusAbility>().OwnerEntity.UnListenActionPoint(actionPointType, OnActionPointTrigger);
        }

        private void OnActionPointTrigger(Entity combatAction)
        {
            GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }


    }
}