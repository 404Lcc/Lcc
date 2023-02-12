namespace LccModel
{
    public class EffectAssignActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public CombatEntity OwnerEntity => GetParent<CombatEntity>();



        public bool TryMakeAction(out EffectAssignAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChildren<EffectAssignAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
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
        public CombatEntity Creator { get; set; }
        public CombatEntity Target { get; set; }


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