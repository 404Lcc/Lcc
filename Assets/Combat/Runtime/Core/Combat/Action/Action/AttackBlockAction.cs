namespace LccModel
{
    public class AttackBlockActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public CombatEntity OwnerEntity => GetParent<CombatEntity>();



        public override void Awake()
        {
            OwnerEntity.ListenActionPoint(ActionPointType.PreReceiveAttackEffect, TryBlock);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            OwnerEntity.UnListenActionPoint(ActionPointType.PreReceiveAttackEffect, TryBlock);
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
            if (TryGetComponent(out AbilityProbabilityTriggerComponent component))
            {
                return RandomUtil.RandomNumber(0, 10000) < component.probability;
            }
            return false;
        }

        public void TryBlock(Entity action)
        {
            if (IsAbilityEffectTrigger())
            {
                if (TryMakeAction(out var attackBlockAction))
                {
                    attackBlockAction.ActionAbility = this;
                    attackBlockAction.attackExecution = ((SpellAttackAction)action).attackExecution;
                    attackBlockAction.ApplyBlock();
                }
            }
        }
    }

    public class AttackBlockAction : Entity, IActionExecution
    {
        public AttackExecution attackExecution;

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

        }

        public void ApplyBlock()
        {
            PreProcess();

            attackExecution.SetBlocked();

            PostProcess();

            FinishAction();
        }

        private void PostProcess()
        {
        }
    }
}