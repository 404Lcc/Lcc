using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 
    /// </summary>
    public class EffectCustomComponent : Component
    {
        public override bool DefaultEnable => false;


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        public override void OnEnable()
        {
            if (GetParent<AbilityEffect>().EffectConfig is CustomEffect customEffect)
            {
                if (customEffect.CustomEffectType == "强体")
                {
                    var probabilityTriggerComponent = GetParent<AbilityEffect>().OwnerEntity.AttackBlockAbility.AddComponent<AbilityProbabilityTriggerComponent>();
                    var param = customEffect.Params.First().Value;
                    probabilityTriggerComponent.Probability = (int)(float.Parse(param.Replace("%", "")) * 100);
                }
            }
        }

        private void OnAssignEffect(Entity entity)
        {

        }
    }
}