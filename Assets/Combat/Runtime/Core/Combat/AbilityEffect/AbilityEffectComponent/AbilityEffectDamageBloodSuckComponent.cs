using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccModel
{
    public class AbilityEffectDamageBloodSuckComponent : Component
    {

        public override void Awake()
        {
            base.Awake();
            GetParent<AbilityEffect>().OwnerEntity.damageActionAbility.AddComponent<DamageBloodSuckComponent>();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (GetParent<AbilityEffect>().OwnerEntity.damageActionAbility.TryGetComponent<DamageBloodSuckComponent>(out var damageBloodSuckComponent))
            {
                damageBloodSuckComponent.Dispose();
            }

        }

    }
}