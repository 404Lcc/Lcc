using System.Collections.Generic;

namespace LccModel
{
    public class AttackAbility : Entity, IAbilityEntity
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public CombatEntity ParentEntity { get => GetParent<CombatEntity>(); }
        public bool Enable { get; set; }


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            var effects = new List<Effect>();
            var damageEffect = new DamageEffect();
            damageEffect.Enabled = true;
            damageEffect.AddSkillEffectTargetType = AddSkillEffetTargetType.SkillTarget;
            damageEffect.EffectTriggerType = EffectTriggerType.Condition;
            damageEffect.CanCrit = true;
            damageEffect.DamageType = DamageType.Physic;
            damageEffect.DamageValueFormula = $"自身攻击力";
            effects.Add(damageEffect);
            AddComponent<AbilityEffectComponent, List<Effect>>(effects);

        }

        public void DeactivateAbility()
        {
        }

        public void EndAbility()
        {
        }

        public void TryActivateAbility()
        {
            ActivateAbility();
        }

        public void ActivateAbility()
        {
            Enable = true;
        }

        public Entity CreateExecution()
        {
            var execution = OwnerEntity.AddChildren<AttackExecution>(this);
            execution.AbilityEntity = this;
            return execution;
        }
    }
}