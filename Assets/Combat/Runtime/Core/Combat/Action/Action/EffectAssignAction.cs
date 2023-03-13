namespace LccModel
{
    public class EffectAssignActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();



        public bool TryMakeAction(out EffectAssignAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = Owner.AddChildren<EffectAssignAction>();
                action.ActionAbility = this;
                action.Creator = Owner;
            }
            return Enable;
        }
    }

    public class EffectAssignAction : Entity, IActionExecution
    {
        public AbilityEffect abilityEffect;

        //�ͷ��������Ч���ж� ��������Skill���� Status���� Item���� Attack������
        public Entity sourceAbility;

        public IActionExecution actionExecution;
        public IAbilityExecution abilityExecution;
        public AbilityItem abilityItem;


        public Entity ActionAbility { get; set; }
        public EffectAssignAction SourceAssignAction { get; set; }
        public Combat Creator { get; set; }
        public Combat Target { get; set; }


        private void PreProcess()
        {

        }

        public void ApplyEffectAssign()
        {
            PreProcess();

            abilityEffect.StartAssignEffect(this);

            PostProcess();

            FinishAction();
        }

        public void FillDatasToAction(IActionExecution action)
        {
            action.SourceAssignAction = this;
            action.Target = Target;
        }

        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.AssignEffect, this);
            Target.TriggerActionPoint(ActionPointType.ReceiveEffect, this);
        }

        public void FinishAction()
        {
            Dispose();
        }
    }
}