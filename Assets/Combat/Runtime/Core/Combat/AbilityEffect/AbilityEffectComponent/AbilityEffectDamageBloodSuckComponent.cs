namespace LccModel
{
    public class AbilityEffectDamageBloodSuckComponent : Component
    {
        public Combat OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;
        public override void Awake()
        {
            base.Awake();
            OwnerEntity.damageActionAbility.AddComponent<DamageBloodSuckComponent>();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            var component = OwnerEntity.damageActionAbility.GetComponent<DamageBloodSuckComponent>();
            if (component != null)
            {
                component.Dispose();
            }
        }

    }
}