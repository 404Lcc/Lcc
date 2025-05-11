using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public static class TargetUtility
    {
        /// <summary>
        /// entity是否是有效的目标
        /// </summary>
        public static bool IsValidTarget(this LogicEntity entity)
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

        public static LogicEntity GetNearestTarget(LogicEntity entity, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            LogicEntity ret = null;
            if (maxDis <= 0)
            {
                return ret;
            }

            float minSqrDis = maxDis * maxDis;
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (target.hasComSubobject)
                    continue;
                var comFaction = target.comFaction;
                var sameFaction = comFaction.faction == entity.comFaction.faction;
                if (isOtherFaction)
                {
                    if (sameFaction)
                        continue;
                }
                else
                {
                    if (!sameFaction)
                        continue;
                    // 同一个Faction下是否要排除自己
                    if (sameFactionExcludeSelf)
                    {
                        if (target.comID.id == entity.comID.id)
                            continue;
                    }
                }

                // 排除需要主动排除的目标
                if (excludeTargetIdList != null)
                {
                    if (excludeTargetIdList.Contains(target.comID.id))
                    {
                        continue;
                    }
                }

                if (!target.IsValidTarget())
                    continue;

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= minSqrDis)
                {
                    ret = target;
                    minSqrDis = sqrDis;
                }
            }

            return ret;
        }

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
                if (target.hasComSubobject)
                    continue;
                var comFaction = target.comFaction;
                var sameFaction = comFaction.faction == entity.comFaction.faction;
                if (isOtherFaction)
                {
                    if (sameFaction)
                        continue;
                }
                else
                {
                    if (!sameFaction)
                        continue;
                    // 同一个Faction下是否要排除自己
                    if (sameFactionExcludeSelf)
                    {
                        if (target.comID.id == entity.comID.id)
                            continue;
                    }
                }

                // 排除需要主动排除的目标
                if (excludeTargetIdList != null)
                {
                    if (excludeTargetIdList.Contains(target.comID.id))
                    {
                        continue;
                    }
                }

                if (!target.IsValidTarget())
                    continue;

                // 计算到目标距离
                var dir = target.comTransform.position - entity.comTransform.position;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis >= minSqrDis && sqrDis <= maxSqrDis)
                {
                    ret = target;
                    break;
                }
            }

            return ret;
        }

        public static LogicEntity GetHPHightTarget(LogicEntity entity, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false)
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
                if (target.hasComSubobject)
                    continue;
                var comFaction = target.comFaction;
                var sameFaction = comFaction.faction == entity.comFaction.faction;
                if (isOtherFaction)
                {
                    if (sameFaction)
                        continue;
                }
                else
                {
                    if (!sameFaction)
                        continue;
                    // 同一个Faction下是否要排除自己
                    if (sameFactionExcludeSelf)
                    {
                        if (target.comID.id == entity.comID.id)
                            continue;
                    }
                }

                if (!target.IsValidTarget())
                    continue;

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

        public static LogicEntity GetHPPercentLowTarget(LogicEntity entity, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, bool excludeFullHp = true)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            LogicEntity ret = null;
            List<LogicEntity> targetList = new List<LogicEntity>();
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (target.hasComSubobject)
                    continue;
                var comFaction = target.comFaction;
                var sameFaction = comFaction.faction == entity.comFaction.faction;
                if (isOtherFaction)
                {
                    if (sameFaction)
                        continue;
                }
                else
                {
                    if (!sameFaction)
                        continue;
                    // 同一个Faction下是否要排除自己
                    if (sameFactionExcludeSelf)
                    {
                        if (target.comID.id == entity.comID.id)
                            continue;
                    }
                }

                if (!target.IsValidTarget())
                    continue;

                targetList.Add(target);
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

        public static List<LogicEntity> GetNearestTargetList(Vector3 pos, FactionType faction, int targetNum, float maxDis = 10, bool isOtherFaction = true, List<long> excludeTargetIdList = null)
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
                if (target.hasComSubobject)
                    continue;
                var comFaction = target.comFaction;
                var sameFaction = comFaction.faction == faction;
                if (isOtherFaction)
                {
                    if (sameFaction)
                        continue;
                }
                else
                {
                    if (!sameFaction)
                        continue;
                }

                if (!target.IsValidTarget())
                    continue;

                // 排除需要主动排除的目标
                if (excludeTargetIdList != null)
                {
                    if (excludeTargetIdList.Contains(target.comID.id))
                    {
                        continue;
                    }
                }

                // 计算到目标距离
                var dir = target.comTransform.position - pos;
                var sqrDis = dir.sqrMagnitude;
                if (sqrDis <= maxSqrDis)
                {
                    targetList.Add(target);
                }
            }

            // 根据距离排序
            targetList.Sort(SortByDist);

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

        public static List<LogicEntity> GetNearestTargetList(LogicEntity entity, int targetNum, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            Vector3 pos = entity.comTransform.position;
            FactionType faction = entity.comFaction.faction;
            return GetNearestTargetList(pos, faction, targetNum, maxDis, isOtherFaction, excludeTargetIdList);
        }

        public static List<LogicEntity> GetNearestTargetList(LogicEntity entity, int targetNum, Vector3 start, Vector3 dir, float dis2line, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            List<LogicEntity> targetList = new List<LogicEntity>();
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (target.hasComSubobject)
                    continue;
                var comFaction = target.comFaction;
                var sameFaction = comFaction.faction == entity.comFaction.faction;
                if (isOtherFaction)
                {
                    if (sameFaction)
                        continue;
                }
                else
                {
                    if (!sameFaction)
                        continue;
                    // 同一个Faction下是否要排除自己
                    if (sameFactionExcludeSelf)
                    {
                        if (target.comID.id == entity.comID.id)
                            continue;
                    }

                }

                if (!target.IsValidTarget())
                    continue;

                // 排除需要主动排除的目标
                if (excludeTargetIdList != null)
                {
                    if (excludeTargetIdList.Contains(target.comID.id))
                    {
                        continue;
                    }
                }

                //计算距离线的距离
                var dis = DistancePointToRay(start, dir, target.comTransform.position);
                if (dis < dis2line)
                {
                    targetList.Add(target);
                }
            }

            // 根据距离排序
            targetList.Sort(SortByDist);

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

        public static List<LogicEntity> GetHPPercentLowTargetList(LogicEntity entity, int targetNum, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, bool excludeFullHp = true)
        {
            var group = WorldUtility.GetLogicGroup_Faction_Property_Transform();
            List<LogicEntity> ret = new List<LogicEntity>();
            List<LogicEntity> targetList = new List<LogicEntity>();
            var entities = group.GetEntities();
            foreach (var target in entities)
            {
                if (target.hasComSubobject)
                    continue;
                var comFaction = target.comFaction;
                var sameFaction = comFaction.faction == entity.comFaction.faction;
                if (isOtherFaction)
                {
                    if (sameFaction)
                        continue;
                }
                else
                {
                    if (!sameFaction)
                        continue;
                    // 同一个Faction下是否要排除自己
                    if (sameFactionExcludeSelf)
                    {
                        if (target.comID.id == entity.comID.id)
                            continue;
                    }
                }

                if (!target.IsValidTarget())
                    continue;

                targetList.Add(target);
            }

            targetList.Sort(SortByHPPercent);

            var targetLeftCount = targetNum;

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
        /// 根据hp排序
        /// </summary>
        private static int SortByHPPercent(LogicEntity a, LogicEntity b)
        {
            if (a == null || b == null)
                return 0;

            if (!a.hasComHP || !b.hasComHP)
                return 0;

            if (!a.hasComProperty || !b.hasComProperty)
                return 0;

            var aHpPercent = a.comHP.HP / a.comProperty.maxHP;
            var bHpPercent = b.comHP.HP / b.comProperty.maxHP;

            if (aHpPercent < bHpPercent)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// 根据距离排序,目前只是判断z轴
        /// </summary>
        private static int SortByDist(LogicEntity a, LogicEntity b)
        {
            if (a == null || b == null)
                return 0;

            if (!a.hasComTransform || !b.hasComTransform)
                return 0;

            var aDis = a.comTransform.position.x + a.comTransform.position.y + a.comTransform.position.z;
            var bDis = b.comTransform.position.x + b.comTransform.position.y + b.comTransform.position.z;
            return aDis.CompareTo(bDis);
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