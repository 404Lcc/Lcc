namespace LccModel
{
    public class ExecutionEffectSpawnCollisionComponent : Component
    {
        public CollisionExecuteData collisionExecuteData;



        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            Parent.GetParent<SkillExecution>().SpawnCollisionItem(executionEffect.executeClipData);
        }


    }
}