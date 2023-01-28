using UnityEngine;

namespace LccModel
{
    public class AbilityEffectDamageReduceWithTargetCountComponent : Component
    {
        public float ReducePercent { get; set; }
        public float MinPercent { get; set; }


        public override void Awake()
        {
            var damageEffect = (Parent as AbilityEffect).EffectConfig as DamageEffect;
            foreach (var effectDecorator in damageEffect.Decorators)
            {
                if (effectDecorator is DamageReduceWithTargetCountDecorator reduceWithTargetCountDecorator)
                {
                    ReducePercent = reduceWithTargetCountDecorator.ReducePercent / 100;
                    MinPercent = reduceWithTargetCountDecorator.MinPercent / 100;
                }
            }
        }

        public float GetDamagePercent(int TargetCounter)
        {
            return Mathf.Max(MinPercent, 1 - ReducePercent * TargetCounter);
        }
    }
}