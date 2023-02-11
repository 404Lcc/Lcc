using System.Collections.Generic;

namespace LccModel
{
    public class ExecutionEffectComponent : Component
    {
        public List<ExecutionEffect> executionEffectList = new List<ExecutionEffect>();


        public override void Awake()
        {
            if (GetParent<SkillExecution>().executionConfigObject == null)
            {
                return;
            }
            foreach (var effect in GetParent<SkillExecution>().executionConfigObject.ExecuteClipDataList)
            {
                ExecutionEffect executionEffect = Parent.AddChildren<ExecutionEffect, ExecuteClipData>(effect);
                AddEffect(executionEffect);
            }
        }

        public void AddEffect(ExecutionEffect executionEffect)
        {
            executionEffectList.Add(executionEffect);
        }

        public void BeginExecute()
        {
            foreach (var item in executionEffectList)
            {
                item.BeginExecute();
            }
        }
    }
}