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
    /// ����Ч���ж�
    /// </summary>
    public class EffectAssignAction : Entity, IActionExecution
    {
        public AbilityEffect abilityEffect;

        // �ͷ��������Ч���ж� �� ������Skill���� Status���� Item���� Attack������
        public Entity sourceAbility;

        public IActionExecution actionExecution;
        public IAbilityExecution abilityExecution;
        public AbilityItem abilityItem;

        // �ж�����
        public Entity ActionAbility { get; set; }
        // Ч�������ж�Դ
        public EffectAssignAction SourceAssignAction { get; set; }
        // �ж�ʵ��
        public CombatEntity Creator { get; set; }
        // Ŀ�����
        public CombatEntity Target { get; set; }


        // ǰ�ô���
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

        // ���ô���
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