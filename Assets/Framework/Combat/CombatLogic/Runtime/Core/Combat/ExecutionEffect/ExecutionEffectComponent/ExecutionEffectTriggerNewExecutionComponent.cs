namespace LccModel
{
    public class ExecutionEffectTriggerNewExecutionComponent : Component
    {
        public Combat Owner => Parent.GetParent<SkillExecution>().Owner;
        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            ExecutionConfigObject executionObject = Owner.AttachExecution(executionEffect.executeClipData.ActionEventData.NewExecutionId);
            if (executionObject == null)
            {
                return;
            }
            SkillExecution parentExecution = Parent.GetParent<SkillExecution>();

            SkillExecution execution = parentExecution.Owner.AddChildren<SkillExecution, SkillAbility>(parentExecution.SkillAbility);
            execution.executionConfigObject = executionObject;
            execution.inputPoint = parentExecution.inputPoint;
            execution.inputDirection = parentExecution.inputDirection;
            execution.LoadExecutionEffect();
            execution.BeginExecute();
        }
    }
}