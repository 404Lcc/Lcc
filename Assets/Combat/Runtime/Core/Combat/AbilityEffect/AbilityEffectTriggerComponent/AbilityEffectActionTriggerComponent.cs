namespace LccModel
{
    /// <summary>
    /// 行动点触发组件
    /// </summary>
    public class AbilityEffectActionTriggerComponent : Component
    {
        public Effect effect;
        public ActionPointType actionPointType;
        public override void Awake()
        {
            base.Awake();
            effect = GetParent<AbilityEffect>().effectConfig;
            actionPointType = effect.ActionPointType;

            GetParent<AbilityEffect>().GetParent<StatusAbility>().OwnerEntity.ListenActionPoint(actionPointType, OnActionPointTrigger);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            GetParent<AbilityEffect>().GetParent<StatusAbility>().OwnerEntity.UnListenActionPoint(actionPointType, OnActionPointTrigger);
        }

        private void OnActionPointTrigger(Entity combatAction)
        {
            GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }
    }
}