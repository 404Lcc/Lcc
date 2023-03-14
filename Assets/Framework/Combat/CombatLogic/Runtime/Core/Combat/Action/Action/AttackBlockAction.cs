namespace LccModel
{
    public class AttackBlockActionAbility : Entity, IActionAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();



        public override void Awake()
        {
            Owner.ListenActionPoint(ActionPointType.PreReceiveAttackEffect, TryBlock);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Owner.UnListenActionPoint(ActionPointType.PreReceiveAttackEffect, TryBlock);
        }
        public bool TryMakeAction(out AttackBlockAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = Owner.AddChildren<AttackBlockAction>();
                action.ActionAbility = this;
                action.Creator = Owner;
            }
            return Enable;
        }

        private bool IsAbilityEffectTrigger()
        {
            var component = GetComponent<AbilityProbabilityTriggerComponent>();
            if (component != null)
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
        public Combat Creator { get; set; }
        public Combat Target { get; set; }

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