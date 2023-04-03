namespace LccModel
{
    public class ExecutionEffectSpawnCollisionComponent : Component
    {
        public Combat Owner => Parent.GetParent<SkillExecution>().Owner;
        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            Parent.GetParent<SkillExecution>().SpawnCollisionItem(executionEffect.executeClipData);
        }


    }
}