namespace LccModel
{
    public class AbilityEffectDecoratosComponent : Component
    {
        public override void Awake()
        {
            if (GetParent<AbilityEffect>().EffectConfig.Decorators != null)
            {
                foreach (var effectDecorator in GetParent<AbilityEffect>().EffectConfig.Decorators)
                {
                    if (effectDecorator is DamageReduceWithTargetCountDecorator reduceWithTargetCountDecorator)
                    {
                        Parent.AddComponent<AbilityEffectDamageReduceWithTargetCountComponent>();
                    }
                }
            }
        }
    }
}