using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ExecutionEffectComponent : Component
    {
        public List<ExecutionEffect> ExecutionEffects { get; private set; } = new List<ExecutionEffect>();


        public override void Awake()
        {
            if (GetParent<SkillExecution>().ExecutionObject == null)
            {
                return;
            }
            foreach (var effect in GetParent<SkillExecution>().ExecutionObject.ExecuteClips)
            {
                var executionEffect = Parent.AddChildren<ExecutionEffect>(effect);
                AddEffect(executionEffect);
            }
        }

        public void AddEffect(ExecutionEffect executionEffect)
        {
            ExecutionEffects.Add(executionEffect);
        }

        public void BeginExecute()
        {
            foreach (var item in ExecutionEffects)
            {
                item.BeginExecute();
            }
        }
    }
}