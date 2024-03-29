﻿using System.Collections.Generic;

namespace LccModel
{
    public class AbilityEffectComponent : Component
    {
        public List<AbilityEffect> abilityEffectList = new List<AbilityEffect>();
        public AbilityEffect damageAbilityEffect;
        public AbilityEffect cureAbilityEffect;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            if (p1 == null)
            {
                return;
            }
            var effectList = p1 as List<Effect>;
            foreach (Effect item in effectList)
            {
                AbilityEffect abilityEffect = Parent.AddChildren<AbilityEffect, Effect>(item);
                AddEffect(abilityEffect);

                if (abilityEffect.effect is DamageEffect)
                {
                    damageAbilityEffect = abilityEffect;
                }
                if (abilityEffect.effect is CureEffect)
                {
                    cureAbilityEffect = abilityEffect;
                }
            }
        }
        public void EnableEffect()
        {
            foreach (var item in abilityEffectList)
            {
                item.EnableEffect();
            }
        }
        public void AddEffect(AbilityEffect abilityEffect)
        {
            abilityEffectList.Add(abilityEffect);
        }
        public AbilityEffect GetEffect(int index = 0)
        {
            return abilityEffectList[index];
        }
        public void TryAssignAllEffectToTarget(Combat target)
        {
            if (abilityEffectList.Count > 0)
            {
                foreach (var abilityEffect in abilityEffectList)
                {
                    abilityEffect.TryAssignEffectToTarget(target);
                }
            }
        }
        public void TryAssignAllEffectToTarget(Combat target, IActionExecution actionExecution)
        {
            if (abilityEffectList.Count > 0)
            {
                foreach (var abilityEffect in abilityEffectList)
                {
                    abilityEffect.TryAssignEffectToTarget(target, actionExecution);
                }
            }
        }
        public void TryAssignAllEffectToTarget(Combat target, IAbilityExecution abilityExecution)
        {
            if (abilityEffectList.Count > 0)
            {
                foreach (var abilityEffect in abilityEffectList)
                {
                    abilityEffect.TryAssignEffectToTarget(target, abilityExecution);
                }
            }
        }
        public void TryAssignAllEffectToTarget(Combat target, AbilityItem abilityItem)
        {
            if (abilityEffectList.Count > 0)
            {
                foreach (var abilityEffect in abilityEffectList)
                {
                    abilityEffect.TryAssignEffectToTarget(target, abilityItem);
                }
            }
        }
        public void TryAssignEffectToTargetByIndex(Combat target, int index)
        {
            abilityEffectList[index].TryAssignEffectToTarget(target);
        }
    }
}