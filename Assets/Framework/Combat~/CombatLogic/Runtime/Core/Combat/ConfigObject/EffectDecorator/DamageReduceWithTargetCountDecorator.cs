using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LccModel
{
    [Serializable, EffectDecorate("按命中目标数递减百分比伤害", 10)]
    public class DamageReduceWithTargetCountDecorator : EffectDecorator
    {
        [HideInInspector]
        public override string Label => "按命中目标数递减百分比伤害";

        [ToggleGroup("Enabled"), LabelText("递减百分比")]
        public float ReducePercent;
        [ToggleGroup("Enabled"), LabelText("伤害下限百分比")]
        public float MinPercent;
    }
}