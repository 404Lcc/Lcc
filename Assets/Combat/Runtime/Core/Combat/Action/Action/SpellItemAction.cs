namespace LccModel
{
    public class SpellItemActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public bool Enable { get; set; }


        public bool TryMakeAction(out SpellItemAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChildren<SpellItemAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }


    public class SpellItemAction : Entity, IActionExecution
    {
        public ItemAbility itemAbility;

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
            Creator.TriggerActionPoint(ActionPointType.PreGiveItem, this);
            Target.TriggerActionPoint(ActionPointType.PreReceiveItem, this);
        }

        public void UseItem()
        {
            PreProcess();


            itemAbility.GetComponent<AbilityEffectComponent>().TryAssignAllEffectToTarget(Target);


            PostProcess();

            FinishAction();
        }

        //后置处理
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveItem, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveItem, this);
        }
    }
}