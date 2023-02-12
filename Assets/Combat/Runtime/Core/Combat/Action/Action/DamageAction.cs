using UnityEngine;

namespace LccModel
{
    public enum DamageSource
    {
        Attack,
        Skill,
        Buff,
    }
    public class DamageActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public CombatEntity OwnerEntity => GetParent<CombatEntity>();




        public bool TryMakeAction(out DamageAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChildren<DamageAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }


    public class DamageAction : Entity, IActionExecution
    {
        public DamageSource damageSource;
        public int damageValue;

        public Entity ActionAbility { get; set; }
        public EffectAssignAction SourceAssignAction { get; set; }
        public CombatEntity Creator { get; set; }
        public CombatEntity Target { get; set; }


        public void FinishAction()
        {
            Dispose();
        }

        private void PreProcess()
        {
            DamageEffect damageEffect = (DamageEffect)SourceAssignAction.abilityEffect.effect;
            bool isCritical = false;

            if (damageSource == DamageSource.Attack)
            {
                isCritical = (RandomUtil.RandomRate() / 100f) < Creator.GetComponent<AttributeComponent>().CriticalProbability.Value;
                damageValue = Mathf.CeilToInt(Mathf.Max(1, Creator.GetComponent<AttributeComponent>().Attack.Value - Target.GetComponent<AttributeComponent>().Defense.Value));
                if (isCritical)
                {
                    damageValue = Mathf.CeilToInt(damageValue * 1.5f);
                }
            }

            if (damageSource == DamageSource.Skill)
            {
                if (damageEffect.CanCrit)
                {
                    isCritical = (RandomUtil.RandomRate() / 100f) < Creator.GetComponent<AttributeComponent>().CriticalProbability.Value;
                }
                damageValue = SourceAssignAction.abilityEffect.GetComponent<AbilityEffectDamageComponent>().GetDamageValue();
                if (isCritical)
                {
                    damageValue = Mathf.CeilToInt(damageValue * 1.5f);
                }
            }

            if (damageSource == DamageSource.Buff)
            {
                if (damageEffect.CanCrit)
                {
                    isCritical = (RandomUtil.RandomRate() / 100f) < Creator.GetComponent<AttributeComponent>().CriticalProbability.Value;
                }
                damageValue = SourceAssignAction.abilityEffect.GetComponent<AbilityEffectDamageComponent>().GetDamageValue();
                if (isCritical)
                {
                    damageValue = Mathf.CeilToInt(damageValue * 1.5f);
                }
            }

            AbilityEffectDamageReduceWithTargetCountComponent component = SourceAssignAction.abilityEffect.GetComponent<AbilityEffectDamageReduceWithTargetCountComponent>();
            if (component != null)
            {
                if (SourceAssignAction.abilityItem.TryGetComponent(out AbilityItemTargetCounterComponent targetCounterComponent))
                {
                    var damagePercent = component.GetDamagePercent(targetCounterComponent.targetCounter);
                    damageValue = Mathf.CeilToInt(damageValue * damagePercent);
                }
            }


            Creator.TriggerActionPoint(ActionPointType.PreCauseDamage, this);
            Target.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
        }

        public void ApplyDamage()
        {
            PreProcess();

            Target.ReceiveDamage(this);

            PostProcess();

            if (Target.CheckDead())
            {
            }

            FinishAction();
        }

        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
        }
    }
}