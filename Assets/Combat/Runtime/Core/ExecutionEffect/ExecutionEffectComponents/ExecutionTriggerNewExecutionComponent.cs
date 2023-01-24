using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ExecutionTriggerNewExecutionComponent : Component
    {
        public ActionEventData ActionEventData { get; set; }


        public override void Awake()
        {
            ((Entity)Parent).Subscribe<ExecuteEffectEvent>(OnTriggerExecutionEffect);
        }

        public void OnTriggerExecutionEffect(ExecuteEffectEvent evnt)
        {
            ExecutionObject executionObject = null;//AssetUtils.Load<ExecutionObject>(ActionEventData.NewExecution);
            if (executionObject == null)
            {
                return;
            }
            var sourceExecution = Parent.GetParent<SkillExecution>();
            var execution = sourceExecution.OwnerEntity.AddChildren<SkillExecution>(sourceExecution.SkillAbility);
            execution.ExecutionObject = executionObject;
            execution.InputTarget = sourceExecution.InputTarget;
            execution.InputPoint = sourceExecution.InputPoint;
            execution.LoadExecutionEffects();
            execution.BeginExecute();
        }
    }
}