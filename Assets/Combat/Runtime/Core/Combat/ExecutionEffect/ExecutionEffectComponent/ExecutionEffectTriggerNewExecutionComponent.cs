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
            ExecutionObject executionObject = AssetManager.Instance.LoadAsset<ExecutionObject>(out var handler, actionEventData.NewExecution, AssetSuffix.Asset, AssetType.Execution);
            if (executionObject == null)
            {
                return;
            }
            var sourceExecution = Parent.GetParent<SkillExecution>();
            var execution = sourceExecution.OwnerEntity.AddChildren<SkillExecution, SkillAbility>(sourceExecution.skillAbility);
            execution.executionObject = executionObject;
            execution.inputTarget = sourceExecution.inputTarget;
            execution.inputPoint = sourceExecution.inputPoint;
            execution.LoadExecutionEffect();
            execution.BeginExecute();
        }
    }
}