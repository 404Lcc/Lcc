namespace LccModel
{
    public class ExecutionTriggerNewExecutionComponent : Component
    {
        public ActionEventData ActionEventData { get; set; }


        public override void Awake()
        {
            ((Entity)Parent).Subscribe<ExecuteEffectEvent>(OnTriggerExecutionEffect);
        }

        public void OnTriggerExecutionEffect(ExecuteEffectEvent evnt)
        {
            ExecutionObject executionObject = AssetManager.Instance.LoadAsset<ExecutionObject>(out var handler, ActionEventData.NewExecution, AssetSuffix.Asset, AssetType.Execution);
            if (executionObject == null)
            {
                return;
            }
            var sourceExecution = Parent.GetParent<SkillExecution>();
            var execution = sourceExecution.OwnerEntity.AddChildren<SkillExecution, SkillAbility>(sourceExecution.SkillAbility);
            execution.ExecutionObject = executionObject;
            execution.InputTarget = sourceExecution.InputTarget;
            execution.InputPoint = sourceExecution.InputPoint;
            execution.LoadExecutionEffects();
            execution.BeginExecute();
        }
    }
}