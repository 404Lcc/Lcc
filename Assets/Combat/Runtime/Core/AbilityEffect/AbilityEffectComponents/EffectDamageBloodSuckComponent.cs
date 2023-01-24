using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 
    /// </summary>
    public class EffectDamageBloodSuckComponent : Component
    {
        public override bool DefaultEnable => false;


        public override void OnEnable()
        {
            base.OnEnable();
            GetParent<AbilityEffect>().OwnerEntity.DamageAbility.AddComponent<DamageBloodSuckComponent>();

        }
        public override void OnDisable()
        {
            base.OnDisable();

            if (GetParent<AbilityEffect>().OwnerEntity.DamageAbility.TryGetComponent<DamageBloodSuckComponent>(out var damageBloodSuckComponent))
            {
                damageBloodSuckComponent.Dispose();
            }
        }
    }
}