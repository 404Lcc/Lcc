namespace LccModel
{
    public class ExecutionEffectAssignToTargetComponent : Component
    {
        public override bool DefaultEnable => false;
        public EffectApplyType effectApplyType;


        public override void Awake()
        {
            ((Entity)Parent).Subscribe<ExecutionEffect>(OnTriggerExecuteEffect);
        }

        public void OnTriggerExecuteEffect(ExecutionEffect executionEffect)
        {
            var ParentExecution = Parent.GetParent<SkillExecution>();

            if (ParentExecution.inputTarget != null)
            {
                if (effectApplyType == EffectApplyType.AllEffects)
                {
                    ParentExecution.AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignAllEffectToTarget(ParentExecution.inputTarget, ParentExecution);
                }
                else
                {
                    ParentExecution.AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignEffectToTargetByIndex(ParentExecution.inputTarget, (int)effectApplyType - 1);
                }
            }
        }
    }
}