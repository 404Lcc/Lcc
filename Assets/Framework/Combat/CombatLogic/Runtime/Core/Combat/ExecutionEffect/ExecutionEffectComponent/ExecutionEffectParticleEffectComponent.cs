namespace LccModel
{
    public class ExecutionEffectParticleEffectComponent : Component
    {
        public Combat Owner => Parent.GetParent<SkillExecution>().Owner;


        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            EventManager.Instance.Publish(new SyncParticleEffect(Owner.InstanceId, executionEffect.executeClipData.ParticleEffectData.ParticleEffectName, Owner.TransformComponent.position, Owner.TransformComponent.rotation));
        }

        public void OnTriggerExecutionEffectEnd(ExecutionEffect executionEffect)
        {
            EventManager.Instance.Publish(new SyncDeleteParticleEffect(Owner.InstanceId, executionEffect.executeClipData.ParticleEffectData.ParticleEffectName));
        }
    }
}