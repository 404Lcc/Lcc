namespace LccModel
{
    public class EffectAssignActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }


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

    /// <summary>
    /// 赋给效果行动
    /// </summary>
    public class EffectAssignAction : Entity, IActionExecution
    {
        public AbilityEffect abilityEffect;

        // 释放这个赋予效果行动 的 能力（Skill能力 Status能力 Item能力 Attack能力）
        public Entity sourceAbility;

        public IActionExecution actionExecution;
        public IAbilityExecution abilityExecution;
        public AbilityItem abilityItem;

        // 行动能力
        public Entity ActionAbility { get; set; }
        // 效果赋给行动源
        public EffectAssignAction SourceAssignAction { get; set; }
        // 行动实体
        public CombatEntity Creator { get; set; }
        // 目标对象
        public CombatEntity Target { get; set; }


        // 前置处理
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

        // 后置处理
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