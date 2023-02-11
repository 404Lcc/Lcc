using UnityEngine;

namespace LccModel
{
    public class ExecutionEffectAnimationComponent : Component
    {
        public CombatEntity OwnerEntity => Parent.GetParent<SkillExecution>().OwnerEntity;
        public AnimationClip animationClip;


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.TriggerEffect), OnTriggerExecutionEffect);
        }

        public void OnTriggerExecutionEffect(Entity entity)
        {
            OwnerEntity.Publish(animationClip);
        }
    }
}