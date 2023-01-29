namespace LccModel
{
    public class SpellAttackActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }


        public bool TryMakeAction(out SpellAttackAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChildren<SpellAttackAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    /// <summary>
    /// 普攻行动
    /// </summary>
    public class SpellAttackAction : Entity, IActionExecution
    {
        public AttackExecution attackExecution;

        // 行动能力
        public Entity ActionAbility { get; set; }
        // 效果赋给行动源
        public EffectAssignAction SourceAssignAction { get; set; }
        // 行动实体
        public CombatEntity Creator { get; set; }
        // 目标对象
        public CombatEntity Target { get; set; }


        public void FinishAction()
        {
            Dispose();
        }

        //前置处理
        private void PreProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PreGiveAttack, this);
            Target.TriggerActionPoint(ActionPointType.PreReceiveAttack, this);
        }

        public void ApplyAttack()
        {
            PreProcess();

            attackExecution = Creator.attackAbility.CreateExecution() as AttackExecution;
            attackExecution.attackAction = this;
            attackExecution.BeginExecute();

            PostProcess();

            FinishAction();
        }


        //后置处理
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveAttack, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveAttack, this);
        }
    }
}