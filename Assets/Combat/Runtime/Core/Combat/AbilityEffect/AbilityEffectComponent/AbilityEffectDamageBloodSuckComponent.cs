using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccModel
{
    public class AbilityEffectDamageBloodSuckComponent : Component
    {
        public override bool DefaultEnable => false;


        public override void OnEnable()
        {
            base.OnEnable();
            GetParent<AbilityEffect>().OwnerEntity.damageActionAbility.AddComponent<DamageBloodSuckComponent>();

        }
        public override void OnDisable()
        {
            base.OnDisable();

            if (GetParent<AbilityEffect>().OwnerEntity.damageActionAbility.TryGetComponent<DamageBloodSuckComponent>(out var damageBloodSuckComponent))
            {
                damageBloodSuckComponent.Dispose();
            }
        }
    }
}