namespace LccModel
{
    public class ExecutionEffectParticleEffectComponent : Component
    {
        public Combat OwnerEntity => Parent.GetParent<SkillExecution>().OwnerEntity;


        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            EventManager.Instance.Publish(new SyncParticleEffect(OwnerEntity.InstanceId, executionEffect.executeClipData.ParticleEffectData.ParticleEffectName, OwnerEntity.TransformComponent.position, OwnerEntity.TransformComponent.rotation)).Coroutine();
        }

        public void OnTriggerExecutionEffectEnd(ExecutionEffect executionEffect)
        {
            EventManager.Instance.Publish(new SyncDeleteParticleEffect(OwnerEntity.InstanceId, executionEffect.executeClipData.ParticleEffectData.ParticleEffectName)).Coroutine();
        }
    }
}