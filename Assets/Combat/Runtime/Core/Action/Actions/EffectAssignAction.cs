namespace LccModel
{
    public class EffectAssignAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
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
    /// ����Ч���ж�
    /// </summary>
    public class EffectAssignAction : Entity, IActionExecution
    {
        /// �������Ч�������ж���Դ����
        public Entity SourceAbility { get; set; }
        /// Ŀ���ж�
        public IActionExecution TargetAction { get; set; }
        public AbilityEffect AbilityEffect { get; set; }
        public AbilityItem AbilityItem { get; set; }
        public Effect EffectConfig => AbilityEffect.EffectConfig;
        /// �ж�����
        public Entity ActionAbility { get; set; }
        /// Ч�������ж�Դ
        public EffectAssignAction SourceAssignAction { get; set; }
        /// �ж�ʵ��
        public CombatEntity Creator { get; set; }
        /// Ŀ�����
        public CombatEntity Target { get; set; }


        /// ǰ�ô���
        private void PreProcess()
        {

        }

        public void ApplyEffectAssign()
        {
            PreProcess();

            AbilityEffect.StartAssignEffect(this);

            PostProcess();

            FinishAction();
        }

        public void FillDatasToAction(IActionExecution action)
        {
            action.SourceAssignAction = this;
            action.Target = Target;
        }

        /// ���ô���
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