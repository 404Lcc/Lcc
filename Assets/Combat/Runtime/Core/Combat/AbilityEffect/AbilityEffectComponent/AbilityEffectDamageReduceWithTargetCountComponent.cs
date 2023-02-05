using UnityEngine;

namespace LccModel
{
    public class AbilityEffectDamageReduceWithTargetCountComponent : Component
    {
        public DamageEffect damageEffect;

        public float reducePercent;
        public float minPercent;


        public override void Awake()
        {
            damageEffect = (Parent as AbilityEffect).effectConfig as DamageEffect;

            foreach (var item in damageEffect.DecoratorList)
            {
                if (item is DamageReduceWithTargetCountDecorator reduceWithTargetCountDecorator)
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