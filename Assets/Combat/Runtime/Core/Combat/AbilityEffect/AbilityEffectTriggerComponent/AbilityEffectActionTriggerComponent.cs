namespace LccModel
{
    public class AbilityEffectActionTriggerComponent : Component
    {
        public Effect Effect => GetParent<AbilityEffect>().effect;
        public ActionPointType ActionPointType => Effect.ActionPointType;

        public CombatEntity OwnerEntity => GetParent<AbilityEffect>().GetParent<StatusAbility>().OwnerEntity;

        public override void Awake()
        {
            OwnerEntity.ListenActionPoint(ActionPointType, OnActionPointTrigger);
        }

        public override void OnDestroy()
        {
            OwnerEntity.UnListenActionPoint(ActionPointType, OnActionPointTrigger);
        }

        private void OnActionPointTrigger(Entity action)
        {
            GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }
    }
}