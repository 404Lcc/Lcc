namespace LccModel
{
    public class AbilityEffectDecoratosComponent : Component
    {
        public override void Awake()
        {
            if (GetParent<AbilityEffect>().effectConfig.DecoratorList != null)
            {
                foreach (var effectDecorator in GetParent<AbilityEffect>().effectConfig.DecoratorList)
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