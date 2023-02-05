using System.Linq;

namespace LccModel
{
    public class AbilityEffectCustomComponent : Component
    {
        public CustomEffect customEffect;

        public override void Awake()
        {
            base.Awake();

            customEffect = (CustomEffect)GetParent<AbilityEffect>().effectConfig;

            if (customEffect.CustomEffectType == "强体")
            {
                var probabilityTriggerComponent = GetParent<AbilityEffect>().OwnerEntity.attackBlockActionAbility.AddComponent<AbilityProbabilityTriggerComponent>();
                var param = customEffect.ParamsDict.First().Value;
                probabilityTriggerComponent.probability = (int)(float.Parse(param.Replace("%", "")) * 100);
            }

        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (GetParent<AbilityEffect>().OwnerEntity.attackBlockActionAbility.TryGetComponent<AbilityProbabilityTriggerComponent>(out var abilityProbabilityTriggerComponent))
            {
                abilityProbabilityTriggerComponent.Dispose();
            }
        }

    }
}