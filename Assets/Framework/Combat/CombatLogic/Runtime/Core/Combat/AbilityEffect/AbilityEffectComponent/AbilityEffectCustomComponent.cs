using System.Linq;

namespace LccModel
{
    public class AbilityEffectCustomComponent : Component
    {
        public CustomEffect CustomEffect => (CustomEffect)GetParent<AbilityEffect>().effect;
        public Combat Owner => GetParent<AbilityEffect>().Owner;

        public override void Awake()
        {
            if (CustomEffect.CustomEffectType == "强体")
            {
                AbilityProbabilityTriggerComponent abilityProbabilityTriggerComponent = GetParent<AbilityEffect>().Owner.attackBlockActionAbility.AddComponent<AbilityProbabilityTriggerComponent>();
                abilityProbabilityTriggerComponent.probability = (int)(float.Parse(CustomEffect.ParamsDict.First().Value.Replace("%", "")) * 100);
            }

        }
        public override void OnDestroy()
        {
            var component = Owner.attackBlockActionAbility.GetComponent<AbilityProbabilityTriggerComponent>();
            if (component != null)
            {
                component.Dispose();
            }
        }

    }
}