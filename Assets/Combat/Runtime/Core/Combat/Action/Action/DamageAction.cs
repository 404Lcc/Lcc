using UnityEngine;

namespace LccModel
{
    public class DamageActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }



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

    /// <summary>
    /// 伤害行动
    /// </summary>
    public class DamageAction : Entity, IActionExecution
    {
        public DamageEffect damageEffect;
        // 伤害来源
        public DamageSource damageSource;


        public int damageValue;

        public bool isCritical;


        // 行动能力
        public Entity ActionAbility { get; set; }
        // 效果赋给行动源
        public EffectAssignAction SourceAssignAction { get; set; }
        // 行动实体
        public CombatEntity Creator { get; set; }
        // 目标对象
        public CombatEntity Target { get; set; }


        public void FinishAction()
        {
            Dispose();
        }

        // 前置处理
        private void PreProcess()
        {
            damageEffect = SourceAssignAction.abilityEffect.effectConfig as DamageEffect;

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
            }

            var executionDamageReduceWithTargetCountComponent = SourceAssignAction.abilityEffect.GetComponent<AbilityEffectDamageReduceWithTargetCountComponent>();
            if (executionDamageReduceWithTargetCountComponent != null)
            {
                if (SourceAssignAction.abilityItem.TryGetComponent(out AbilityItemTargetCounterComponent targetCounterComponent))
                {
                    var damagePercent = executionDamageReduceWithTargetCountComponent.GetDamagePercent(targetCounterComponent.targetCounter);
                    damageValue = Mathf.CeilToInt(damageValue * damagePercent);
                }
            }

            //触发 造成伤害前 行动点
            Creator.TriggerActionPoint(ActionPointType.PreCauseDamage, this);
            //触发 承受伤害前 行动点
            Target.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
        }

        // 应用伤害
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

        // 后置处理
        private void PostProcess()
        {
            //触发 造成伤害后 行动点
            Creator.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
            //触发 承受伤害后 行动点
            Target.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
        }
    }

    public enum DamageSource
    {
        Attack,// 普攻
        Skill,// 技能
        Buff,// Buff
    }
}