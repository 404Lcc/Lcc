using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 索敌距离和目标半径修正相关扩展。
    /// </summary>
    public static class LogicEntityTargetingExtensions
    {
        /// <summary>
        /// 获取实体到目标的索敌修正距离，目标半径会从距离中扣除。
        /// </summary>
        public static float GetTargetingDistance(this LogicEntity entity, LogicEntity target)
        {
            return entity.comTransform.position.GetTargetingDistance(target);
        }

        /// <summary>
        /// 获取指定位置到目标的索敌修正距离，目标半径会从距离中扣除。
        /// </summary>
        public static float GetTargetingDistance(this Vector3 pos, LogicEntity target)
        {
            var dir = target.comTransform.position - pos;
            if (target.hasComBounds)
            {
                return Mathf.Max(0, dir.magnitude - target.comBounds.GetRadius());
            }

            return dir.magnitude;
        }
    }
}