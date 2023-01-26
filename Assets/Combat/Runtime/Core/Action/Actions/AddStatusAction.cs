namespace LccModel
{
    public class AddStatusActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public bool Enable { get; set; }


        public bool TryMakeAction(out AddStatusAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChildren<AddStatusAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    /// <summary>
    /// ʩ��״̬�ж�
    /// </summary>
    public class AddStatusAction : Entity, IActionExecution
    {
        public Entity SourceAbility { get; set; }
        public AddStatusEffect AddStatusEffect => SourceAssignAction.AbilityEffect.EffectConfig as AddStatusEffect;
        public StatusAbility Status { get; set; }

        /// �ж�����
        public Entity ActionAbility { get; set; }
        /// Ч�������ж�Դ
        public EffectAssignAction SourceAssignAction { get; set; }
        /// �ж�ʵ��
        public CombatEntity Creator { get; set; }
        /// Ŀ�����
        public CombatEntity Target { get; set; }


        public void FinishAction()
        {
            Dispose();
        }

        //ǰ�ô���
        private void PreProcess()
        {

        }

        public void ApplyAddStatus()
        {
            PreProcess();

            var statusConfig = AddStatusEffect.AddStatus;
            var canStack = statusConfig.CanStack;

            if (canStack == false)
            {
                if (Target.HasStatus(statusConfig.ID))
                {
                    var status = Target.GetStatus(statusConfig.ID);
                    var statusLifeTimer = status.GetComponent<StatusLifeTimeComponent>().LifeTimer;
                    statusLifeTimer.MaxTime = AddStatusEffect.Duration / 1000f;
                    statusLifeTimer.Reset();
                    return;
                }
            }

            Status = Target.AttachStatus(statusConfig);
            Status.OwnerEntity = Creator;
            Status.GetComponent<AbilityLevelComponent>().Level = SourceAbility.GetComponent<AbilityLevelComponent>().Level;
            Status.Duration = (int)AddStatusEffect.Duration;
    

            Status.ProcessInputKVParams(AddStatusEffect.Params);

            Status.AddComponent<StatusLifeTimeComponent>();
            Status.TryActivateAbility();

            PostProcess();

            FinishAction();
        }

        //���ô���
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveStatus, this);
            Target.TriggerActionPoint(ActionPointType.PostReceiveStatus, this);
        }
    }
}