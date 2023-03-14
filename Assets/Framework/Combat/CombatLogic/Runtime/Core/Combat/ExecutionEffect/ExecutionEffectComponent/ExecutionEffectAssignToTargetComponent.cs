namespace LccModel
{
    public class ExecutionEffectAssignToTargetComponent : Component
    {
        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            SkillExecution parentExecution = Parent.GetParent<SkillExecution>();

            if (parentExecution.inputTarget != null)
            {
                if (executionEffect.executeClipData.ActionEventData.EffectApply == EffectApplyType.AllEffects)
                {
                    parentExecution.Ability.GetComponent<AbilityEffectComponent>().TryAssignAllEffectToTarget(parentExecution.inputTarget, parentExecution);
                }
                else
                {
                    parentExecution.Ability.GetComponent<AbilityEffectComponent>().TryAssignEffectToTargetByIndex(parentExecution.inputTarget, (int)executionEffect.executeClipData.ActionEventData.EffectApply - 1);
                }
            }
        }
    }
}