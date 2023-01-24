using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ExecutionSpawnCollisionComponent : Component
    {
        public CollisionExecuteData CollisionExecuteData { get; set; }


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.TriggerEffect), OnTriggerExecutionEffect);
            ((Entity)Parent).OnEvent(nameof(ExecutionEffect.EndEffect), OnTriggerEnd);
        }

        public void OnTriggerExecutionEffect(Entity entity)
        {
#if !NOT_UNITY
            Parent.GetParent<SkillExecution>().SpawnCollisionItem(GetParent<ExecutionEffect>().ExecutionEffectConfig);
#endif
        }

        public void OnTriggerEnd(Entity entity)
        {
        }
    }
}