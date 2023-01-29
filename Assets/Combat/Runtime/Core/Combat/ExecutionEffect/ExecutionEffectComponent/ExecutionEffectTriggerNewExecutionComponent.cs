namespace LccModel
{
    public class ExecutionEffectTriggerNewExecutionComponent : Component
    {
        public ActionEventData actionEventData;


        public override void Awake()
        {
            ((Entity)Parent).Subscribe<ExecuteEffectEvent>(OnTriggerExecutionEffect);
        }

        public void OnTriggerExecutionEffect(ExecuteEffectEvent evnt)
        {
            ExecutionConfigObject executionObject = AssetManager.Instance.LoadAsset<ExecutionConfigObject>(out var handler, actionEventData.NewExecution, AssetSuffix.Asset, AssetType.Execution);
            if (executionObject == null)
            {
                return;
            }
            var sourceExecution = Parent.GetParent<SkillExecution>();
            var execution = sourceExecution.OwnerEntity.AddChildren<SkillExecution, SkillAbility>(sourceExecution.SkillAbility);
            execution.executionConfigObject = executionObject;
            execution.inputTarget = sourceExecution.inputTarget;
            execution.inputPoint = sourceExecution.inputPoint;
            execution.LoadExecutionEffect();
            execution.BeginExecute();
        }
    }
}