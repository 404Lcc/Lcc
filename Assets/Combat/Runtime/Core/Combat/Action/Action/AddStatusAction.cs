namespace LccModel
{
    public class AddStatusActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
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
        public Entity sourceAbility;
        public AddStatusEffect addStatusEffect;
        public StatusAbility status;

        // �ж�����
        public Entity ActionAbility { get; set; }
        // Ч�������ж�Դ
        public EffectAssignAction SourceAssignAction { get; set; }
        // �ж�ʵ��
        public CombatEntity Creator { get; set; }
        // Ŀ�����
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

            addStatusEffect = SourceAssignAction.abilityEffect.effectConfig as AddStatusEffect;

            var statusConfig = addStatusEffect.StatusConfigObject;
            var canStack = statusConfig.CanStack;

            if (canStack == false)
            {
                if (Target.HasStatus(statusConfig.Id))
                {
                    var status = Target.GetStatus(statusConfig.Id);
                    var statusLifeTimer = status.GetComponent<StatusLifeTimeComponent>().lifeTimer;
                    statusLifeTimer.MaxTime = addStatusEffect.Duration / 1000f;
                    statusLifeTimer.Reset();
                    return;
                }
            }

            status = Target.AttachStatus(statusConfig);
            status.OwnerEntity = Creator;
            status.GetComponent<AbilityLevelComponent>().level = sourceAbility.GetComponent<AbilityLevelComponent>().level;
            status.duration = (int)addStatusEffect.Duration;


            status.ProcessInputKVParams(addStatusEffect.ParamsDict);

            status.AddComponent<StatusLifeTimeComponent>();
            status.TryActivateAbility();

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