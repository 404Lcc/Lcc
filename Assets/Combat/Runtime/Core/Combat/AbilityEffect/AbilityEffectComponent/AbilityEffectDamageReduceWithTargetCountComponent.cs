using UnityEngine;

namespace LccModel
{
    public class AbilityEffectDamageReduceWithTargetCountComponent : Component
    {
        public float reducePercent;
        public float minPercent;


        public override void Awake()
        {
            var damageEffect = (Parent as AbilityEffect).effectConfig as DamageEffect;
            foreach (var effectDecorator in damageEffect.DecoratorList)
            {
                if (effectDecorator is DamageReduceWithTargetCountDecorator reduceWithTargetCountDecorator)
                {
                    reducePercent = reduceWithTargetCountDecorator.ReducePercent / 100;
                    minPercent = reduceWithTargetCountDecorator.MinPercent / 100;
                }
            }
        }

        public float GetDamagePercent(int TargetCounter)
        {
            return Mathf.Max(minPercent, 1 - reducePercent * TargetCounter);
        }
    }
}