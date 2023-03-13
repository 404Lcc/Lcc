namespace LccModel
{
    public class SpellAttackActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();



        public bool TryMakeAction(out SpellAttackAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = Owner.AddChildren<SpellAttackAction>();
                action.ActionAbility = this;
                action.Creator = Owner;
            }
            return Enable;
        }
    }

    public class SpellAttackAction : Entity, IActionExecution
    {
        public AttackExecution attackExecution;

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
            Creator.TriggerActionPoint(ActionPointType.PreGiveAttack, this);
            Target.TriggerActionPoint(ActionPointType.PreReceiveAttack, this);
        }

        public void ApplyAttack()
        {
            PreProcess();

            attackExecution = (AttackExecution)Creator.attackAbility.CreateExecution();
            attackExecution.attackAction = this;
            attackExecution.BeginExecute();

            PostProcess();

            FinishAction();
        }

        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveAttack, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveAttack, this);
        }
    }
}