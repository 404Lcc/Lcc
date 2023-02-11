using System.Linq;

namespace LccModel
{
    public class AbilityEffectCustomComponent : Component
    {
        public CustomEffect CustomEffect => (CustomEffect)GetParent<AbilityEffect>().effect;
        public CombatEntity OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;

        public override void Awake()
        {
            if (CustomEffect.CustomEffectType == "强体")
            {
                AbilityProbabilityTriggerComponent abilityProbabilityTriggerComponent = GetParent<AbilityEffect>().OwnerEntity.attackBlockActionAbility.AddComponent<AbilityProbabilityTriggerComponent>();
                abilityProbabilityTriggerComponent.probability = (int)(float.Parse(CustomEffect.ParamsDict.First().Value.Replace("%", "")) * 100);
            }

        }
        public override void OnDestroy()
        {
            if (OwnerEntity.attackBlockActionAbility.TryGetComponent<AbilityProbabilityTriggerComponent>(out var component))
            {
                component.Dispose();
            }
        }

    }
}