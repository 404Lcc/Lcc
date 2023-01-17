namespace LccModel
{
    /// <summary>
    /// 普攻执行体
    /// </summary>
    public class AttackExecution : Entity, IAbilityExecution, IUpdate
    {
        public AttackAction AttackAction { get; set; }
        public Entity AbilityEntity { get; set; }
        public CombatEntity OwnerEntity { get; set; }

        private bool BeBlocked;//是否被格挡
        private bool HasTriggerDamage;//是否触发了伤害


        public void Update()
        {
            if (!HasTriggerDamage)
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
            BeBlocked = true;
        }

        public void BeginExecute()
        {
        }

        /// <summary>
        /// 前置处理
        /// </summary>
        private void PreProcess()
        {
            AttackAction.Creator.TriggerActionPoint(ActionPointType.PreGiveAttackEffect, AttackAction);
            AttackAction.Target.TriggerActionPoint(ActionPointType.PreReceiveAttackEffect, AttackAction);
        }

        /// <summary>
        /// 尝试触发普攻效果
        /// </summary>
        private void TryTriggerAttackEffect()
        {
            HasTriggerDamage = true;

            PreProcess();

            if (BeBlocked)
            {
                //Log.Debug("被格挡了");
            }
            else
            {
                AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignAllEffectsToTargetWithExecution(AttackAction.Target, this);
            }
        }

        public void EndExecute()
        {
            AttackAction.FinishAction();
            AttackAction = null;
            Dispose();
        }
    }
}