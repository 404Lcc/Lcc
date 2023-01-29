using UnityEngine;

namespace LccModel
{
    public class ExecutionEffectParticleEffectComponent : Component
    {
        public GameObject particleEffectPrefab;
        public GameObject particleEffect;


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.TriggerEffect), OnTriggerStart);
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.EndEffect), OnTriggerEnd);
        }

        public void OnTriggerStart(Entity entity)
        {
            particleEffect = GameObject.Instantiate(particleEffectPrefab, Parent.GetParent<SkillExecution>().OwnerEntity.Position, Parent.GetParent<SkillExecution>().OwnerEntity.Rotation);
        }

        public void OnTriggerEnd(Entity entity)
        {
            GameObject.Destroy(particleEffect);
        }
    }
}