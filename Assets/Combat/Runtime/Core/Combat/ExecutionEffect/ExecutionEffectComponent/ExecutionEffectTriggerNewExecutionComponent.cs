namespace LccModel
{
    public class ExecutionEffectTriggerNewExecutionComponent : Component
    {
        public ActionEventData actionEventData;


        public override void Awake()
        {
            ((Entity)Parent).Subscribe<ExecutionEffect>(OnTriggerExecutionEffect);
        }

        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            ExecutionConfigObject executionObject = AssetManager.Instance.LoadAsset<ExecutionConfigObject>(out var handler, actionEventData.NewExecution, AssetSuffix.Asset, AssetType.Execution);
            if (executionObject == null)
            {
                return;
            }
            SkillExecution parentExecution = Parent.GetParent<SkillExecution>();

            SkillExecution execution = parentExecution.OwnerEntity.AddChildren<SkillExecution, SkillAbility>(parentExecution.SkillAbility);
            execution.executionConfigObject = executionObject;
            execution.inputTarget = parentExecution.inputTarget;
            execution.inputPoint = parentExecution.inputPoint;
            execution.LoadExecutionEffect();
            execution.BeginExecute();
        }
    }
}