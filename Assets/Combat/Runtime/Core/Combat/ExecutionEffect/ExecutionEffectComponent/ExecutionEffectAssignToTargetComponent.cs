namespace LccModel
{
    public class ExecutionEffectAssignToTargetComponent : Component
    {
        public EffectApplyType effectApplyType;


        public override void Awake()
        {
            ((Entity)Parent).Subscribe<ExecutionEffect>(OnTriggerExecuteEffect);
        }

        public void OnTriggerExecuteEffect(ExecutionEffect executionEffect)
        {
            SkillExecution parentExecution = Parent.GetParent<SkillExecution>();

            if (parentExecution.inputTarget != null)
            {
                if (effectApplyType == EffectApplyType.AllEffects)
                {
                    parentExecution.AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignAllEffectToTarget(parentExecution.inputTarget, parentExecution);
                }
                else
                {
                    parentExecution.AbilityEntity.GetComponent<AbilityEffectComponent>().TryAssignEffectToTargetByIndex(parentExecution.inputTarget, (int)effectApplyType - 1);
                }
            }
        }
    }
}