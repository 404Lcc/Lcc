namespace LccModel
{
    public class ExecutionEffectTriggerNewExecutionComponent : Component
    {
        public ActionEventData actionEventData;




        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            ExecutionConfigObject executionObject = AssetManager.Instance.LoadAsset<ExecutionConfigObject>(out var handler, executionEffect.executeClipData.ActionEventData.NewExecution, AssetSuffix.Asset, AssetType.Execution);
            if (executionObject == null)
            {
                return;
            }
            SkillExecution parentExecution = Parent.GetParent<SkillExecution>();

            SkillExecution execution = parentExecution.Owner.AddChildren<SkillExecution, SkillAbility>(parentExecution.SkillAbility);
            execution.executionConfigObject = executionObject;
            execution.inputTarget = parentExecution.inputTarget;
            execution.inputPoint = parentExecution.inputPoint;
            execution.LoadExecutionEffect();
            execution.BeginExecute();
        }
    }
}