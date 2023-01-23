namespace LccModel
{
    public class AttackBlockActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public bool Enable { get; set; }


        public override void Awake()
        {
            OwnerEntity.ListenActionPoint(ActionPointType.PreReceiveAttackEffect, TryBlock);
        }

        public bool TryMakeAction(out AttackBlockAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChildren<AttackBlockAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }

        private bool IsAbilityEffectTrigger()
        {
            if (TryGetComponent(out AbilityProbabilityTriggerComponent probabilityTriggerComponent))
            {
                var r = RandomHelper.RandomNumber(0, 10000);
                return r < probabilityTriggerComponent.Probability;
            }
            return false;
        }

        public void TryBlock(Entity action)
        {
            //Log.Debug($"TryBlock");
            if (IsAbilityEffectTrigger())
            {
                if (TryMakeAction(out var attackBlockAction))
                {
                    attackBlockAction.ActionAbility = this;
                    attackBlockAction.AttackExecution = (action as AttackAction).AttackExecution;
                    attackBlockAction.ApplyBlock();
                }
            }
        }
    }

    /// <summary>
    /// 格挡行动
    /// </summary>
    public class AttackBlockAction : Entity, IActionExecution
    {
        /// 行动能力
        public Entity ActionAbility { get; set; }
        /// 效果赋给行动源
        public EffectAssignAction SourceAssignAction { get; set; }
        /// 行动实体
        public CombatEntity Creator { get; set; }
        /// 目标对象
        public CombatEntity Target { get; set; }
        public AttackExecution AttackExecution { get; set; }


        public void FinishAction()
        {
            Dispose();
        }

        //前置处理
        private void PreProcess()
        {

        }

        public void ApplyBlock()
        {
            PreProcess();

            AttackExecution.SetBlocked();

            PostProcess();

            FinishAction();
        }

        //后置处理
        private void PostProcess()
        {
        }
    }
}