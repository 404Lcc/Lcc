namespace LccModel
{
    public class ExecutionEffectSpawnCollisionComponent : Component
    {
        public CollisionExecuteData CollisionExecuteData { get; set; }


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.TriggerEffect), OnTriggerExecutionEffect);
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.EndEffect), OnTriggerEnd);
        }

        public void OnTriggerExecutionEffect(Entity entity)
        {
            Parent.GetParent<SkillExecution>().SpawnCollisionItem(GetParent<ExecutionEffect>().ExecutionEffectConfig);
        }

        public void OnTriggerEnd(Entity entity)
        {
        }
    }
}