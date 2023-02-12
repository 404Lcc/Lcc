namespace LccModel
{
    public class AbilityEffectActionControlComponent : Component
    {
        public ActionControlEffect ActionControlEffect => (ActionControlEffect)GetParent<AbilityEffect>().effect;
        public CombatEntity OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;
        public StatusAbility OwnerAbility => (StatusAbility)GetParent<AbilityEffect>().OwnerAbility;

        public override void Awake()
        {
            OwnerEntity.GetComponent<StatusComponent>().OnStatusesChanged(OwnerAbility);
        }

        public override void OnDestroy()
        {
            OwnerEntity.GetComponent<StatusComponent>().OnStatusesChanged(OwnerAbility);
        }
    }
}