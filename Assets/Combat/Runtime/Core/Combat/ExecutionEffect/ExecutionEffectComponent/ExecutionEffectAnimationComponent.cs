using UnityEngine;

namespace LccModel
{
    public class ExecutionEffectAnimationComponent : Component
    {
        public AnimationClip animationClip;


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.TriggerEffect), OnTriggerExecutionEffect);
        }

        public void OnTriggerExecutionEffect(Entity entity)
        {
            Parent.GetParent<SkillExecution>().OwnerEntity.Publish(animationClip);
        }
    }
}