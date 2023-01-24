using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 
    /// </summary>
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
            //Log.Debug("ExecutionAnimationComponent OnTriggerExecutionEffect");
            //Entity.GetParent<SkillExecution>().OwnerEntity.Publish(AnimationClip);
            ParticleEffectObj = GameObject.Instantiate(ParticleEffectPrefab, Parent.GetParent<SkillExecution>().OwnerEntity.Position, Parent.GetParent<SkillExecution>().OwnerEntity.Rotation);
        }

        public void OnTriggerEnd(Entity entity)
        {
            //Log.Debug("ExecutionAnimationComponent OnTriggerExecutionEffect");
            //Entity.GetParent<SkillExecution>().OwnerEntity.Publish(AnimationClip);
            GameObject.Destroy(ParticleEffectObj);
        }
    }
}