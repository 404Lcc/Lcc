namespace LccModel
{
    public class ExecutionEffectAssignToTargetComponent : Component
    {
        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            SkillExecution parentExecution = Parent.GetParent<SkillExecution>();

            if (parentExecution.targetList.Count > 0)
            {
                if (executionEffect.executeClipData.ActionEventData.EffectApply == EffectApplyType.AllEffects)
                {
                    foreach (var item in parentExecution.targetList)
                    {
                        parentExecution.Ability.GetComponent<AbilityEffectComponent>().TryAssignAllEffectToTarget(item, parentExecution);
                    }
                }
                else
                {
                    foreach (var item in parentExecution.targetList)
                    {
                        parentExecution.Ability.GetComponent<AbilityEffectComponent>().TryAssignEffectToTargetByIndex(item, (int)executionEffect.executeClipData.ActionEventData.EffectApply - 1);
                    }
                }
            }
        }
    }
}