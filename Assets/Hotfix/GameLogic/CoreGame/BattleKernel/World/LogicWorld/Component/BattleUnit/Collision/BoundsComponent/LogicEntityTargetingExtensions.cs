using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 索敌距离和目标半径修正相关扩展。
    /// </summary>
    public static class LogicEntityTargetingExtensions
    {
        /// <summary>
        /// 获取实体到目标的索敌修正距离，自身和目标半径会从距离中扣除。
        /// </summary>
        public static float GetTargetingDistance(this LogicEntity entity, LogicEntity target)
        {
            var distance = (target.comTransform.position - entity.position).magnitude;
            if (entity.hasComBounds)
            {
                distance = Mathf.Max(0, distance - entity.comBounds.GetRadius());
            }
            if (target.hasComBounds)
            {
                distance = Mathf.Max(0, distance - target.comBounds.GetRadius());
            }
            return distance;
        }

        /// <summary>
        /// 获取指定位置到目标的索敌修正距离，自身和目标半径会从距离中扣除。
        /// </summary>
        public static float GetTargetingDistance(this Vector3 pos, LogicEntity target)
        {
            var distance = (target.comTransform.position - pos).magnitude;
            if (target.hasComBounds)
            {
                distance = Mathf.Max(0, distance - target.comBounds.GetRadius());
            }

            return distance;
        }
    }
}