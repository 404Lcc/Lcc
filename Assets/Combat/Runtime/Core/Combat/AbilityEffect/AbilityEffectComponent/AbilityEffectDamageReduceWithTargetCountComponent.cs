using UnityEngine;

namespace LccModel
{
    public class AbilityEffectDamageReduceWithTargetCountComponent : Component
    {
        public DamageEffect DamageEffect => (DamageEffect)GetParent<AbilityEffect>().effect;

        public float reducePercent;
        public float minPercent;


        public override void Awake()
        {
            foreach (var item in DamageEffect.DecoratorList)
            {
                if (item is DamageReduceWithTargetCountDecorator decorator)
                {
                    reducePercent = decorator.ReducePercent / 100;
                    minPercent = decorator.MinPercent / 100;
                }
            }
        }

        public float GetDamagePercent(int targetCounter)
        {
            return Mathf.Max(minPercent, 1 - reducePercent * targetCounter);
        }
    }
}