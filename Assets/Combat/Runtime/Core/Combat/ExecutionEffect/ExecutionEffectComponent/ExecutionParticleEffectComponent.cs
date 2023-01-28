using UnityEngine;

namespace LccModel
{
    public class ExecutionParticleEffectComponent : Component
    {
        public GameObject ParticleEffectPrefab { get; set; }
        public GameObject ParticleEffectObj { get; set; }


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.TriggerEffect), OnTriggerStart);
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.EndEffect), OnTriggerEnd);
        }

        public void OnTriggerStart(Entity entity)
        {
            ParticleEffectObj = GameObject.Instantiate(ParticleEffectPrefab, Parent.GetParent<SkillExecution>().OwnerEntity.Position, Parent.GetParent<SkillExecution>().OwnerEntity.Rotation);
        }

        public void OnTriggerEnd(Entity entity)
        {
            GameObject.Destroy(ParticleEffectObj);
        }
    }
}