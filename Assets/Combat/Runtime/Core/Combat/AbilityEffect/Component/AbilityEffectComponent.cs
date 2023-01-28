using System.Collections.Generic;

namespace LccModel
{
    public class AbilityEffectComponent : Component
    {
        public override bool DefaultEnable { get; set; } = false;
        public List<AbilityEffect> AbilityEffects { get; private set; } = new List<AbilityEffect>();
        public AbilityEffect DamageAbilityEffect { get; set; }
        public AbilityEffect CureAbilityEffect { get; set; }

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            if (p1 == null)
            {
                return;
            }
            var effects = p1 as List<Effect>;
            foreach (var item in effects)
            {
                var abilityEffect = Parent.AddChildren<AbilityEffect, Effect>(item);
                AddEffect(abilityEffect);

                if (abilityEffect.EffectConfig is DamageEffect)
                {
                    DamageAbilityEffect = abilityEffect;
                }
                if (abilityEffect.EffectConfig is CureEffect)
                {
                    CureAbilityEffect = abilityEffect;
                }
            }
        }

        public override void OnEnable()
        {
            foreach (var item in AbilityEffects)
            {
                item.EnableEffect();
            }
        }

        public override void OnDisable()
        {
            foreach (var item in AbilityEffects)
            {
                item.DisableEffect();
            }
        }

        public void AddEffect(AbilityEffect abilityEffect)
        {
            AbilityEffects.Add(abilityEffect);
        }

        public AbilityEffect GetEffect(int index = 0)
        {
            return AbilityEffects[index];
        }

        public void TryAssignAllEffectsToTarget(CombatEntity targetEntity)
        {
            if (AbilityEffects.Count > 0)
            {
                foreach (var abilityEffect in AbilityEffects)
                {
                    abilityEffect.TryAssignEffectTo(targetEntity);
                }
            }
        }
        /// <summary>
        /// 尝试将所有效果赋给目标
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="execution"></param>
        public void TryAssignAllEffectsToTargetWithExecution(CombatEntity targetEntity, IAbilityExecution execution)
        {
            if (AbilityEffects.Count > 0)
            {
                foreach (var abilityEffect in AbilityEffects)
                {
                    abilityEffect.TryAssignEffectTo(targetEntity);
                }
            }
        }

        /// <summary>
        /// 尝试将所有效果赋给目标
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="abilityItem"></param>
        public void TryAssignAllEffectsToTargetWithAbilityItem(CombatEntity targetEntity, AbilityItem abilityItem)
        {
            if (AbilityEffects.Count > 0)
            {
                foreach (var abilityEffect in AbilityEffects)
                {
                    abilityEffect.TryAssignEffectToTargetWithAbilityItem(targetEntity, abilityItem);
                }
            }
        }

        public void TryAssignEffectByIndex(CombatEntity targetEntity, int index)
        {
            AbilityEffects[index].TryAssignEffectTo(targetEntity);
        }
    }
}