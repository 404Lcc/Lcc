namespace LccModel
{
    public class CureActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();



        public bool TryMakeAction(out CureAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = Owner.AddChildren<CureAction>();
                action.ActionAbility = this;
                action.Creator = Owner;
            }
            return Enable;
        }
    }

    public class CureAction : Entity, IActionExecution
    {
        public int cureValue;

        public Entity ActionAbility { get; set; }
        public EffectAssignAction SourceAssignAction { get; set; }
        public Combat Creator { get; set; }
        public Combat Target { get; set; }


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