namespace LccModel
{
    /// <summary>
    /// 执行体应用目标效果组件
    /// </summary>
    public class ExecutionEffectAssignToTargetComponent : Component
    {
        public override bool DefaultEnable => false;
        public EffectApplyType EffectApplyType { get; set; }


        public override void Awake()
        {
            ((Entity)Parent).Subscribe<ExecuteEffectEvent>(OnTriggerExecuteEffect);
        }

        public void OnTriggerExecuteEffect(ExecuteEffectEvent evnt)
        {
            var ParentExecution = Parent.GetParent<SkillExecution>();

            if (ParentExecution.InputTarget != null)
            {
                if (EffectApplyType == EffectApplyType.AllEffects)
                {
                    ParentExecution.AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignAllEffectsToTargetWithExecution(ParentExecution.InputTarget, ParentExecution);
                }
                else
                {
                    ParentExecution.AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignEffectByIndex(ParentExecution.InputTarget, (int)EffectApplyType - 1);
                }
            }
        }
    }
}