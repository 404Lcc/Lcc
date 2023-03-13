namespace LccModel
{
    public class EffectAssignActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public Combat OwnerEntity => GetParent<Combat>();



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

        //释放这个赋予效果行动 的能力（Skill能力 Status能力 Item能力 Attack能力）
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