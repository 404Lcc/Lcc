namespace LccModel
{
    public class AttackExecution : Entity, IAbilityExecution, IUpdate
    {
        public Entity AbilityEntity { get; set; }
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }


        public SpellAttackAction attackAction;
        private bool beBlocked;//是否被格挡
        private bool hasTriggerDamage;//是否触发了伤害


        public void Update()
        {
            if (!hasTriggerDamage)
            {
                TryTriggerAttackEffect();
            }
            else
            {
                this.EndExecute();
            }
        }

        public void SetBlocked()
        {
            beBlocked = true;
        }

        public void BeginExecute()
        {
            GetParent<CombatEntity>().spellingAttackExecution = this;
        }

        /// <summary>
        /// 前置处理
        /// </summary>
        private void PreProcess()
        {
            attackAction.Creator.TriggerActionPoint(ActionPointType.PreGiveAttackEffect, attackAction);
            attackAction.Target.TriggerActionPoint(ActionPointType.PreReceiveAttackEffect, attackAction);
        }

        /// <summary>
        /// 尝试触发普攻效果
        /// </summary>
        private void TryTriggerAttackEffect()
        {
            hasTriggerDamage = true;

            PreProcess();

            if (beBlocked)
            {
            }
            else
            {
                AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignAllEffectToTarget(attackAction.Target, this);
            }
        }

        public void EndExecute()
        {
            GetParent<CombatEntity>().spellingAttackExecution = null;
            attackAction.FinishAction();
            attackAction = null;
            Dispose();
        }
    }
}