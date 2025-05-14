using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public static class TargetUtility
    {
        /// <summary>
        /// entity是否是有效的目标
        /// </summary>
        private static bool IsValidTarget(this LogicEntity entity)
        {
            if (entity == null)
                return false;

            // 血条判断
            if (!entity.hasComHP)
                return false;

            // 属性判断
            if (entity.hasComProperty)
            {
                var comProp = entity.comProperty;
                if (!comProp.isTargetable) // 不可被瞄准
                    return false;
                if (!comProp.isBlockable) // 不可碰撞
                    return false;
                if (!comProp.isAlive) // 已经死亡
                    return false;
                if (!comProp.isDieable && !comProp.isDamageable) // 不可死亡
                    return false;
            }

            // 模型与碰撞判断
            return entity.hasComView;
        }

        /// <summary>
        /// 基础筛选
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="target"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        private static bool BaseFilter(LogicEntity entity, LogicEntity target, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            if (target.hasComSubobject)
                return false;
            var comFaction = target.comFaction;
            var sameFaction = comFaction.faction == entity.comFaction.faction;
            if (isOtherFaction)
            {
                if (sameFaction)
                    return false;
            }
            else
            {
                if (!sameFaction)
                    return false;
                // 同一个Faction下是否要排除自己
                if (sameFactionExcludeSelf)
                {
                    if (target.comID.id == entity.comID.id)
                        return false;
                }
            }

            // 排除需要主动排除的目标
            if (excludeTargetIdList != null)
            {
                if (excludeTargetIdList.Contains(target.comID.id))
                {
                    return false;
                }
            }

            if (!target.IsValidTarget())
                return false;

            return true;
        }

        /// <summary>
        /// 基础筛选
        /// </summary>
        /// <param name="selfEntityId"></param>
        /// <param name="selfFaction"></param>
        /// <param name="target"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        private static bool BaseFilter(long selfEntityId, FactionType selfFaction, LogicEntity target, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            if (target.hasComSubobject)
                return false;
            var comFaction = target.comFaction;
            var sameFaction = comFaction.faction == selfFaction;
            if (isOtherFaction)
            {
                if (sameFaction)
                    return false;
            }
            else
            {
                if (!sameFaction)
                    return false;
                // 同一个Faction下是否要排除自己
                if (sameFactionExcludeSelf)
                {
                    if (target.comID.id == selfEntityId)
                        return false;
                }
            }

            // 排除需要主动排除的目标
            if (excludeTargetIdList != null)
            {
                if (excludeTargetIdList.Contains(target.comID.id))
                {
                    return false;
                }
            }

            if (!target.IsValidTarget())
                return false;

            return true;
        }

        /// <summary>
        /// 找到前方最近的1个敌人
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="maxRange"></param>
        /// <param name="maxDis"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        public static LogicEntity GetForwardNearestTarget(LogicEntity entity, float maxRange = 90, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            LogicEntity ret = null;
            if (maxDis <= 0)
            {
                return ret;
            }

            float maxSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(entity, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= maxSqrDis)
                {
                    var forwardDir = entity.comTransform.rotation * (Vector3.right * entity.comTransform.dirX);
                    // 计算角度
                    float angleToTarget = Vector3.Angle(forwardDir, dir.normalized);
                    if (angleToTarget <= maxRange / 2f)
                    {
                        ret = target;
                        maxSqrDis = sqrDis;
                    }
                }
            }

            return ret;
        }
        
        /// <summary>
        /// 找到范围内最近的1个敌人
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="maxDis"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        public static LogicEntity GetNearestTarget(LogicEntity entity, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            LogicEntity ret = null;
            if (maxDis <= 0)
            {
                return ret;
            }

            float maxSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(entity, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= maxSqrDis)
                {
                    ret = target;
                    maxSqrDis = sqrDis;
                }
            }

            return ret;
        }

        /// <summary>
        /// 找到范围内最近的1个敌人
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="minDis"></param>
        /// <param name="maxDis"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        public static LogicEntity GetNearestTarget(LogicEntity entity, float minDis = 10, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            LogicEntity ret = null;
            if (maxDis <= 0)
            {
                return ret;
            }

            float minSqrDis = minDis * minDis;
            float maxSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(entity, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis >= minSqrDis && sqrDis <= maxSqrDis)
                {
                    ret = target;
                    maxSqrDis = sqrDis;
                }
            }

            return ret;
        }

        /// <summary>
        /// 找到范围内固定血量最高的1个敌人
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="maxDis"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        public static LogicEntity GetHPHightTarget(LogicEntity entity, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            LogicEntity ret = null;
            List<LogicEntity> targetList = new List<LogicEntity>();
            if (maxDis <= 0)
            {
                return ret;
            }

            float maxSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(entity, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= maxSqrDis)
                {
                    targetList.Add(target);
                }
            }

            float HP = float.MinValue;
            for (int i = 0; i < targetList.Count; i++)
            {
                var comHP = targetList[i].comHP;
                if (comHP.HP > HP)
                {
                    ret = targetList[i];
                    HP = comHP.HP;
                }
            }

            return ret;
        }

        /// <summary>
        /// 找到范围内血量百分比最低的1个敌人
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="maxDis"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <param name="excludeFullHp"></param>
        /// <returns></returns>
        public static LogicEntity GetHPPercentLowTarget(LogicEntity entity, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null, bool excludeFullHp = true)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            LogicEntity ret = null;
            List<LogicEntity> targetList = new List<LogicEntity>();
            if (maxDis <= 0)
            {
                return ret;
            }

            float maxSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(entity, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= maxSqrDis)
                {
                    targetList.Add(target);
                }
            }

            float HPPercent = float.MaxValue;
            for (int i = 0; i < targetList.Count; i++)
            {
                var comProp = targetList[i].comProperty;
                var comHP = targetList[i].comHP;
                var targetHPPercent = comHP.HP / comProp.maxHP;
                var isFullHp = comHP.HP >= comProp.maxHP;
                if (targetHPPercent < HPPercent)
                {
                    if (!excludeFullHp && isFullHp)
                        continue;
                    ret = targetList[i];
                    HPPercent = targetHPPercent;
                }
            }

            return ret;
        }
        
        /// <summary>
        /// 找到前方范围内最近的n个敌人
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="targetNum"></param>
        /// <param name="maxRange"></param>
        /// <param name="maxDis"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        public static List<LogicEntity> GetForwardNearestTargetList(LogicEntity entity, int targetNum, float maxRange = 90, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            List<LogicEntity> targetList = new List<LogicEntity>();
            if (maxDis <= 0)
            {
                return targetList;
            }

            float maxSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(entity, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= maxSqrDis)
                {
                    var forwardDir = entity.comTransform.rotation * (Vector3.right * entity.comTransform.dirX);
                    // 计算角度
                    float angleToTarget = Vector3.Angle(forwardDir, dir.normalized);
                    if (angleToTarget <= maxRange / 2f)
                    {
                        targetList.Add(target);
                    }
                }
            }

            // 根据距离排序
            targetList = targetList.OrderBy(x => (x.comTransform.position - entity.comTransform.position).sqrMagnitude).ToList();

            var targetLeftCount = targetNum;
            var ret = new List<LogicEntity>();
            for (int i = 0; i < targetList.Count; i++)
            {
                targetLeftCount--;
                ret.Add(targetList[i]);

                if (targetLeftCount <= 0)
                    break;
            }

            return ret;
        }
        
        /// <summary>
        /// 找到范围内最近的n个敌人
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="targetNum"></param>
        /// <param name="maxDis"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        public static List<LogicEntity> GetNearestTargetList(LogicEntity entity, int targetNum, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            List<LogicEntity> targetList = new List<LogicEntity>();
            if (maxDis <= 0)
            {
                return targetList;
            }

            float maxSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(entity, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= maxSqrDis)
                {
                    targetList.Add(target);
                }
            }

            // 根据距离排序
            targetList = targetList.OrderBy(x => (x.comTransform.position - entity.comTransform.position).sqrMagnitude).ToList();

            var targetLeftCount = targetNum;
            var ret = new List<LogicEntity>();
            for (int i = 0; i < targetList.Count; i++)
            {
                targetLeftCount--;
                ret.Add(targetList[i]);

                if (targetLeftCount <= 0)
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 找到范围内最近的n个敌人
        /// </summary>
        /// <param name="selfPos"></param>
        /// <param name="selfEntityId"></param>
        /// <param name="selfFaction"></param>
        /// <param name="targetNum"></param>
        /// <param name="maxDis"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        public static List<LogicEntity> GetNearestTargetList(Vector3 selfPos, long selfEntityId, FactionType selfFaction, int targetNum, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            List<LogicEntity> targetList = new List<LogicEntity>();
            if (maxDis <= 0)
            {
                return targetList;
            }

            float maxSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(selfEntityId, selfFaction, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                // 计算到目标距离
                var dir = target.comTransform.position - selfPos;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= maxSqrDis)
                {
                    targetList.Add(target);
                }
            }

            // 根据距离排序
            targetList = targetList.OrderBy(x => (x.comTransform.position - selfPos).sqrMagnitude).ToList();

            var targetLeftCount = targetNum;
            var ret = new List<LogicEntity>();
            for (int i = 0; i < targetList.Count; i++)
            {
                targetLeftCount--;
                ret.Add(targetList[i]);

                if (targetLeftCount <= 0)
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 找到距离线最近的n个敌人
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="targetNum"></param>
        /// <param name="start"></param>
        /// <param name="dir"></param>
        /// <param name="dis2line"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <returns></returns>
        public static List<LogicEntity> GetNearestTargetList(LogicEntity entity, int targetNum, Vector3 start, Vector3 dir, float dis2line, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            List<LogicEntity> targetList = new List<LogicEntity>();
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(entity, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                //计算距离线的距离
                var dis = DistancePointToRay(start, dir, target.comTransform.position);
                if (dis < dis2line)
                {
                    targetList.Add(target);
                }
            }

            // 根据距离排序
            targetList = targetList.OrderBy(x => (x.comTransform.position - entity.comTransform.position).sqrMagnitude).ToList();

            var targetLeftCount = targetNum;
            var ret = new List<LogicEntity>();
            for (int i = 0; i < targetList.Count; i++)
            {
                targetLeftCount--;
                ret.Add(targetList[i]);

                if (targetLeftCount <= 0)
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 找到范围内血量百分比最低的n个敌人
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="targetNum"></param>
        /// <param name="maxDis"></param>
        /// <param name="isOtherFaction"></param>
        /// <param name="sameFactionExcludeSelf"></param>
        /// <param name="excludeTargetIdList"></param>
        /// <param name="excludeFullHp"></param>
        /// <returns></returns>
        public static List<LogicEntity> GetHPPercentLowTargetList(LogicEntity entity, int targetNum, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null, bool excludeFullHp = true)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            List<LogicEntity> targetList = new List<LogicEntity>();
            if (maxDis <= 0)
            {
                return targetList;
            }

            float maxSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (!BaseFilter(entity, target, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList))
                {
                    continue;
                }

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= maxSqrDis)
                {
                    targetList.Add(target);
                }
            }

            // 根据血量百分比排序
            targetList = targetList.OrderBy(x => x.comHP.HP / x.comProperty.maxHP).ToList();

            var targetLeftCount = targetNum;
            var ret = new List<LogicEntity>();
            for (int i = 0; i < targetList.Count; i++)
            {
                targetLeftCount--;
                var comProp = targetList[i].comProperty;
                var comHP = targetList[i].comHP;
                var isFullHp = comHP.HP >= comProp.maxHP;
                if (!excludeFullHp && isFullHp)
                    continue;
                ret.Add(targetList[i]);

                if (targetLeftCount <= 0)
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 返回点到射线上最接近点的距离
        /// </summary>
        private static float DistancePointToRay(Vector3 rayStart, Vector3 rayDirection, Vector3 point)
        {
            // 射线参数t = ((point - rayStart) · rayDirection) / (rayDirection · rayDirection)
            float t = Vector3.Dot(point - rayStart, rayDirection) / Vector3.Dot(rayDirection, rayDirection);

            // 使用参数t找到射线上最接近点P的点
            Vector3 closestPointOnRay = rayStart + t * rayDirection;

            // 返回点到射线上最接近点的距离
            return Vector3.Distance(point, closestPointOnRay);
        }
    }
}