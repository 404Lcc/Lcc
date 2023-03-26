using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class ExecutionEffectFiltrationTargetComponent : Component
    {
        public void OnTriggerExecutionEffect(ExecutionEffect executionEffect)
        {
            SkillExecution parentExecution = Parent.GetParent<SkillExecution>();


            parentExecution.targetList.Clear();
            parentExecution.targetList.AddRange(SelectCombat());
        }
        public List<Combat> SelectCombat()
        {
            List<Combat> list = new List<Combat>();
            //foreach (var item in CombatContext.Instance.combatDict)
            //{
            //    Vector3 pos = transform.InverseTransformPoint(item.transform.position);
            //    if (pos.z > 0)
            //    {
            //        float distance = Vector3.Distance(transform.position, item.transform.position);
            //        if (distance <= fightdata.distance)
            //        {
            //            arraylist.Add(item);
            //        }
            //    }
            //}
            return list;
        }
    }
}