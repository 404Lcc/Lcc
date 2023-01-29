using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccModel
{
    public class AbilityEffectCustomComponent : Component
    {
        public override bool DefaultEnable => false;


        public override void OnEnable()
        {
            if (GetParent<AbilityEffect>().effectConfig is CustomEffect customEffect)
            {
                if (customEffect.CustomEffectType == "强体")
                {
                    var probabilityTriggerComponent = GetParent<AbilityEffect>().OwnerEntity.attackBlockActionAbility.AddComponent<AbilityProbabilityTriggerComponent>();
                    var param = customEffect.ParamsDict.First().Value;
                    probabilityTriggerComponent.probability = (int)(float.Parse(param.Replace("%", "")) * 100);
                }
            }
        }

    }
}