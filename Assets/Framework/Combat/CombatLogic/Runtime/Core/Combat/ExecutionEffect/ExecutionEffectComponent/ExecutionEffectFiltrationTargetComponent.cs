using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class ExecutionEffectFiltrationTargetComponent : Component
    {
        public Combat Owner => Parent.GetParent<SkillExecution>().Owner;
        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            SkillExecution parentExecution = Parent.GetParent<SkillExecution>();


            parentExecution.targetList.Clear();
            parentExecution.targetList.AddRange(SelectCombat(executionEffect.executeClipData));
        }
        public List<Combat> SelectCombat(ExecuteClipData executeClipData)
        {
            return FiltrationTarget.GetTargetList(Owner.TransformComponent, executeClipData.ActionEventData.Distance, executeClipData.ActionEventData.TagType);
        }
    }
}