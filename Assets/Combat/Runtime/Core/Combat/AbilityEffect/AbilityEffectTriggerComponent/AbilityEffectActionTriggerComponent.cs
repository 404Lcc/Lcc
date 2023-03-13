namespace LccModel
{
    public class AbilityEffectActionTriggerComponent : Component
    {
        public Effect Effect => GetParent<AbilityEffect>().effect;
        public ActionPointType ActionPointType => Effect.ActionPointType;

        public Combat Owner => GetParent<AbilityEffect>().Owner;

        public override void Awake()
        {
            Owner.ListenActionPoint(ActionPointType, OnActionPointTrigger);
        }

        public override void OnDestroy()
        {
            Owner.UnListenActionPoint(ActionPointType, OnActionPointTrigger);
        }

        private void OnActionPointTrigger(Entity action)
        {
            GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }
    }
}