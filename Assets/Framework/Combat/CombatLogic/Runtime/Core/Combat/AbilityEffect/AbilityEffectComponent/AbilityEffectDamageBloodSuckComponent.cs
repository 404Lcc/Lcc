namespace LccModel
{
    public class AbilityEffectDamageBloodSuckComponent : Component
    {
        public Combat Owner => GetParent<AbilityEffect>().Owner;
        public override void Awake()
        {
            base.Awake();
            Owner.damageActionAbility.AddComponent<DamageBloodSuckComponent>();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            var component = Owner.damageActionAbility.GetComponent<DamageBloodSuckComponent>();
            if (component != null)
            {
                component.Dispose();
            }
        }

    }
}