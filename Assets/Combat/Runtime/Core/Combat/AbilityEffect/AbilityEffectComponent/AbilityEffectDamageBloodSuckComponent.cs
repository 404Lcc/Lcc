namespace LccModel
{
    public class AbilityEffectDamageBloodSuckComponent : Component
    {
        public CombatEntity OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;
        public override void Awake()
        {
            base.Awake();
            OwnerEntity.damageActionAbility.AddComponent<DamageBloodSuckComponent>();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (OwnerEntity.damageActionAbility.TryGetComponent<DamageBloodSuckComponent>(out var component))
            {
                component.Dispose();
            }

        }

    }
}