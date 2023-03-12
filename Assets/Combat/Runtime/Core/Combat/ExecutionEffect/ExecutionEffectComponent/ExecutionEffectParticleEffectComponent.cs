using UnityEngine;

namespace LccModel
{
    public class ExecutionEffectParticleEffectComponent : Component
    {
        public CombatEntity OwnerEntity => Parent.GetParent<SkillExecution>().OwnerEntity;

        public GameObject particleEffectPrefab;
        public GameObject particleEffect;


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.TriggerEffect), OnTriggerStart);
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.EndEffect), OnTriggerEnd);
        }

        public void OnTriggerStart(Entity entity)
        {
            particleEffect = GameObject.Instantiate(particleEffectPrefab, OwnerEntity.TransformComponent.position, OwnerEntity.TransformComponent.rotation);
        }

        public void OnTriggerEnd(Entity entity)
        {
            GameObject.Destroy(particleEffect);
        }
    }
}