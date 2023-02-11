namespace LccModel
{
    public class CureActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity => GetParent<CombatEntity>();
        public bool Enable { get; set; }


        public bool TryMakeAction(out CureAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChildren<CureAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    public class CureAction : Entity, IActionExecution
    {
        public int cureValue;

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
            if (SourceAssignAction != null && SourceAssignAction.abilityEffect != null)
            {
                cureValue = SourceAssignAction.abilityEffect.GetComponent<AbilityEffectCureComponent>().GetCureValue();
            }
        }

        public void ApplyCure()
        {
            PreProcess();

            if (!Target.currentHealth.IsFull())
            {
                Target.ReceiveCure(this);
            }

            PostProcess();

            FinishAction();
        }

        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveCure, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveCure, this);
        }
    }
}