using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ExecutionAnimationComponent : Component
    {
        public AnimationClip AnimationClip { get; set; }


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.TriggerEffect), OnTriggerExecutionEffect);
        }

        public void OnTriggerExecutionEffect(Entity entity)
        {
            //Log.Debug("ExecutionAnimationComponent OnTriggerExecutionEffect");
            Parent.GetParent<SkillExecution>().OwnerEntity.Publish(AnimationClip);
        }
    }
}