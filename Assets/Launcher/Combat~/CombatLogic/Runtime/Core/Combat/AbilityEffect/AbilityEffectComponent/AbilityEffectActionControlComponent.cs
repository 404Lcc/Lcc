namespace LccModel
{
    public class AbilityEffectActionControlComponent : Component
    {
        public ActionControlEffect ActionControlEffect => (ActionControlEffect)GetParent<AbilityEffect>().effect;
        public Combat Owner => GetParent<AbilityEffect>().Owner;
        public StatusAbility OwnerAbility => (StatusAbility)GetParent<AbilityEffect>().OwnerAbility;

        public override void Awake()
        {
            Owner.OnStatusesChanged(OwnerAbility);
        }

        public override void OnDestroy()
        {
            Owner.OnStatusesChanged(OwnerAbility);
        }
    }
}