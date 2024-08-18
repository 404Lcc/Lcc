using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public static class FiltrationTarget
    {
        public static Combat GetTarget(TransformComponent transformComponent, float distance, TagType tagType = TagType.Enemy)
        {
            List<Combat> list = CombatContext.Instance.GetCombatListByTag(tagType);
            if (list.Count == 0) return null;
            float minPos = distance;
            Combat target = null;
            foreach (Combat item in list)
            {
                float temp = Vector3.Distance(item.TransformComponent.position, transformComponent.position);
                if (temp < minPos)
                {
                    minPos = temp;
                    target = item;
                }
            }
            if (target == null) return null;
            return target;
        }
        public static List<Combat> GetTargetList(TransformComponent transformComponent, float distance, TagType tagType = TagType.Enemy)
        {
            List<Combat> targetList = new List<Combat>();
            List<Combat> list = CombatContext.Instance.GetCombatListByTag(tagType);
            if (list.Count == 0) return null;
            foreach (Combat item in list)
            {
                if (IsIncludeTarget(transformComponent, item.TransformComponent))
                {
                    float temp = Vector3.Distance(item.TransformComponent.position, transformComponent.position);
                    if (temp < distance)
                    {
                        targetList.Add(item);
                    }
                }
            }
            return targetList;
        }
        public static bool IsIncludeTarget(TransformComponent self, TransformComponent target)
        {
            Vector3 normalDistance = (target.position - self.position).normalized;
            if (self.rotation.y == 1)
            {
                //自己朝向左边，所以只能打x比我小的
                if (normalDistance.x < 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //自己朝向右边，所以只能打x比我大的
                if (normalDistance.x < 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}