using UnityEngine;

namespace LccModel
{
    public class ExecutionEffectAnimationComponent : Component
    {
        public Combat Owner => Parent.GetParent<SkillExecution>().Owner;



        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            executionEffect.Execution.Owner.AnimationComponent.PlayAnimation(executionEffect.executeClipData.AnimationData.AnimationType);
        }
    }
}