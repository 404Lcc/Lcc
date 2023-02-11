namespace LccModel
{
    public class AbilityEffectDecoratosComponent : Component
    {
        public Effect Effect => GetParent<AbilityEffect>().effect;
        public override void Awake()
        {
            if (Effect.DecoratorList != null)
            {
                foreach (var item in Effect.DecoratorList)
                {
                    if (item is DamageReduceWithTargetCountDecorator)
                    {
                        Parent.AddComponent<AbilityEffectDamageReduceWithTargetCountComponent>();
                    }
                }
            }
        }
    }
}