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
            GetParent<AbilityEffect>().OwnerEntity.DamageActionAbility.AddComponent<DamageBloodSuckComponent>();

        }
        public override void OnDisable()
        {
            base.OnDisable();

            if (GetParent<AbilityEffect>().OwnerEntity.DamageActionAbility.TryGetComponent<DamageBloodSuckComponent>(out var damageBloodSuckComponent))
            {
                damageBloodSuckComponent.Dispose();
            }
        }
    }
}