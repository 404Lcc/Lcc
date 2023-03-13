using UnityEngine;

namespace LccModel
{
    public class ExecutionEffectAnimationComponent : Component
    {
        public Combat OwnerEntity => Parent.GetParent<SkillExecution>().OwnerEntity;



        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            executionEffect.Execution.OwnerEntity.AnimationComponent.PlayAnimation(executionEffect.executeClipData.AnimationData.AnimationType);
        }
    }
}