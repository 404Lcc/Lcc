namespace LccModel
{
    public class AttackExecution : Entity, IAbilityExecution, IUpdate
    {
        public Entity AbilityEntity { get; set; }
        public Combat OwnerEntity => GetParent<Combat>();


        public SpellAttackAction attackAction;
        private bool beBlocked;
        private bool hasTriggerDamage;


        public void Update()
        {
            if (!hasTriggerDamage)
            {
                TryTriggerAttackEffect();
            }
            else
            {
                EndExecute();
            }
        }

        public void SetBlocked()
        {
            beBlocked = true;
        }

        public void BeginExecute()
        {
            OwnerEntity.spellingAttackExecution = this;
        }


        private void PreProcess()
        {
            attackAction.Creator.TriggerActionPoint(ActionPointType.PreGiveAttackEffect, attackAction);
            attackAction.Target.TriggerActionPoint(ActionPointType.PreReceiveAttackEffect, attackAction);
        }


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
            OwnerEntity.spellingAttackExecution = null;
            attackAction.FinishAction();
            attackAction = null;
            Dispose();
        }
    }
}