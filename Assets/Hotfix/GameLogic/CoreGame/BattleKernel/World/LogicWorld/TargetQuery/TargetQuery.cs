using Entitas;
using MackySoft.XPool;
using MackySoft.XPool.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public delegate bool EntityFilter(LogicEntity e);

    public delegate bool EntityAction(LogicEntity e);

    public static partial class TargetQuery
    {
        private static readonly Comparison<KeyValuePair<float, LogicEntity>> s_scoredComparison = (a, b) => a.Key.CompareTo(b.Key);

        /// <summary>
        /// 取索敌候选桶。异阵营仅 Friend、Enemy；同阵营取本桶。游戏双阵营，不扫 Neutral。
        /// </summary>
        private static HashSet<LogicEntity> GetFactionFighterCandidates(LogicWorld logicWorld, EFaction selfFaction, bool isOtherFaction)
        {
            if (!isOtherFaction)
                return logicWorld.GetFactionFighters(selfFaction);

            // 双阵营硬编码：对方只有一个桶
            if (selfFaction == EFaction.Friend)
                return logicWorld.GetFactionFighters(EFaction.Enemy);
            if (selfFaction == EFaction.Enemy)
                return logicWorld.GetFactionFighters(EFaction.Friend);
            return null;
        }

        public static LogicEntity SearchByDistanceY(LogicEntity e, float maxDis = 999, bool isOtherFaction = true, bool sameFactionExcludeSelf = true, List<int> excludeTargetIdList = null, bool cloakTargeting = false)
        {
            LogicEntity ret = null;
            if (maxDis <= 0)
            {
                return ret;
            }

            LogicWorld logicWorld = e.OwnerWorld;
            float curDis = maxDis;
            var pos = e.comTransform.position;
            var selfFaction = e.hasComFaction ? e.comFaction.Faction : EFaction.Invalid;
            var selfCreationIndex = e.creationIndex;

            var candidates = GetFactionFighterCandidates(logicWorld, selfFaction, isOtherFaction);
            if (candidates == null)
                return ret;
            foreach (var target in candidates)
            {
                if (target.hasComSubobject)
                    continue;

                // 同阵营搜时可选排除自己（异阵营桶已不含自身）
                if (!isOtherFaction && sameFactionExcludeSelf && target.creationIndex == selfCreationIndex)
                    continue;

                if (!target.IsValidTarget())
                    continue;

                if (excludeTargetIdList != null && excludeTargetIdList.Contains(target.creationIndex))
                    continue;

                if (!cloakTargeting && target.IsCloaked())
                    continue;

                var dis = pos.GetTargetingDistance(target);
                if (dis <= curDis)
                {
                    ret = target;
                    curDis = dis;
                }
            }

            return ret;
        }


        public static int EntitiesBatchAction(IGroup<LogicEntity> group, EntityFilter filterFuc, EntityAction actionFuc, bool stopOnActionFalse = false, List<LogicEntity> actionEntityList = null)
        {
            int actionCount = 0;
            foreach (var entity in group)
            {
                if (filterFuc(entity))
                {
                    bool res = actionFuc(entity);
                    actionCount++;
                    if (actionEntityList != null)
                    {
                        actionEntityList.Add(entity);
                    }

                    if (!res && stopOnActionFalse)
                    {
                        return actionCount;
                    }
                }
            }

            return actionCount;
        }


        /// <summary>
        /// 基础筛选（阵营已由 FactionFighter 索引保证，此处不再判 Faction）。
        /// </summary>
        private static bool BaseFilter(LogicEntity entity, LogicEntity target, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null, bool cloakTargeting = false)
        {
            return BaseFilter(entity.ID, target, sameFactionExcludeSelf, excludeTargetIdList, cloakTargeting);
        }

        /// <summary>
        /// 基础筛选（阵营已由 FactionFighter 索引保证，此处不再判 Faction）。
        /// </summary>
        private static bool BaseFilter(long selfEntityId, LogicEntity target, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null, bool cloakTargeting = false)
        {
            if (target.hasComSubobject)
                return false;

            // 同阵营搜时可选排除自己
            if (sameFactionExcludeSelf && target.ID == selfEntityId)
                return false;

            if (excludeTargetIdList != null && excludeTargetIdList.Contains(target.ID))
                return false;

            if (!target.IsValidTarget())
                return false;

            if (!cloakTargeting && target.IsCloaked())
                return false;

            return true;
        }

        /// <summary>
        /// 找到范围内最近的1个目标
        /// </summary>
        public static LogicEntity GetNearestTarget(LogicWorld logicWorld, LogicEntity entity, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null, bool cloakTargeting = false)
        {
            LogicEntity ret = null;
            if (maxDis <= 0)
            {
                return ret;
            }

            var selfFaction = entity.comFaction.Faction;
            var candidates = GetFactionFighterCandidates(logicWorld, selfFaction, isOtherFaction);
            if (candidates == null)
                return ret;
            foreach (var target in candidates)
            {
                if (!BaseFilter(entity, target, sameFactionExcludeSelf, excludeTargetIdList, cloakTargeting))
                {
                    continue;
                }

                var dis = entity.GetTargetingDistance(target);
                if (dis <= maxDis)
                {
                    ret = target;
                    maxDis = dis;
                }
            }

            return ret;
        }

        /// <summary>
        /// 找到范围内最近的1个目标（按坐标索敌，支持 exclude）
        /// </summary>
        public static LogicEntity GetNearestTarget(LogicWorld logicWorld, Vector3 selfPos, long selfEntityId, EFaction selfFaction, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null, bool cloakTargeting = false)
        {
            LogicEntity ret = null;
            if (maxDis <= 0)
                return ret;

            var candidates = GetFactionFighterCandidates(logicWorld, selfFaction, isOtherFaction);
            if (candidates == null)
                return ret;

            foreach (var target in candidates)
            {
                if (!BaseFilter(selfEntityId, target, sameFactionExcludeSelf, excludeTargetIdList, cloakTargeting))
                    continue;

                var dis = selfPos.GetTargetingDistance(target);
                if (dis <= maxDis)
                {
                    ret = target;
                    maxDis = dis;
                }
            }

            return ret;
        }

        /// <summary>
        /// 找到范围内最近的 n 个目标，按距离升序写入 results（先 Clear）。无目标时 results 为空而非 null。
        /// 调用方负责 results 生命周期，热路径用 ListPool.RentTemporary。
        /// </summary>
        public static void GetNearestTargetList(LogicWorld logicWorld, LogicEntity entity, List<LogicEntity> results, int targetNum, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null, bool cloakTargeting = false)
        {
            CollectNearest(logicWorld, entity, default, entity.ID, entity.comFaction.Faction,
                targetNum, maxDis, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList, cloakTargeting, results);
        }

        /// <summary>
        /// 找到范围内最近的 n 个目标（按坐标索敌），按距离升序写入 results（先 Clear）。无目标时 results 为空而非 null。
        /// 调用方负责 results 生命周期，热路径用 ListPool.RentTemporary。
        /// </summary>
        public static void GetNearestTargetList(LogicWorld logicWorld, Vector3 selfPos, long selfEntityId, EFaction selfFaction, List<LogicEntity> results, int targetNum, float maxDis = 10, bool isOtherFaction = true, bool sameFactionExcludeSelf = false, List<long> excludeTargetIdList = null, bool cloakTargeting = false)
        {
            CollectNearest(logicWorld, null, selfPos, selfEntityId, selfFaction,
                targetNum, maxDis, isOtherFaction, sameFactionExcludeSelf, excludeTargetIdList, cloakTargeting, results);
        }

        // 评分+截取核心。scored 用 ListPool 租还；结果写入调用方 results。
        private static void CollectNearest(
            LogicWorld logicWorld,
            LogicEntity selfEntity,
            Vector3 selfPos,
            long selfEntityId,
            EFaction selfFaction,
            int targetNum,
            float maxDis,
            bool isOtherFaction,
            bool sameFactionExcludeSelf,
            List<long> excludeTargetIdList,
            bool cloakTargeting,
            List<LogicEntity> results)
        {
            if (results == null)
                return;

            results.Clear();
            if (maxDis <= 0 || targetNum <= 0)
                return;

            var candidates = GetFactionFighterCandidates(logicWorld, selfFaction, isOtherFaction);
            if (candidates == null)
                return;

            using (ListPool<KeyValuePair<float, LogicEntity>>.Shared.RentTemporary(out var scored))
            {
                if (scored.Capacity < candidates.Count)
                    scored.Capacity = candidates.Count;

                var useEntityDistance = selfEntity != null;
                foreach (var target in candidates)
                {
                    if (!BaseFilter(selfEntityId, target, sameFactionExcludeSelf, excludeTargetIdList, cloakTargeting))
                        continue;

                    var dis = useEntityDistance
                        ? selfEntity.GetTargetingDistance(target)
                        : selfPos.GetTargetingDistance(target);
                    if (dis <= maxDis)
                        scored.Add(new KeyValuePair<float, LogicEntity>(dis, target));
                }

                if (scored.Count == 0)
                    return;

                if (targetNum == 1)
                {
                    // 只要最近 1 个：线性找最小，避免全量 Sort
                    var best = scored[0];
                    for (int i = 1; i < scored.Count; i++)
                    {
                        if (scored[i].Key < best.Key)
                            best = scored[i];
                    }

                    results.Add(best.Value);
                    return;
                }

                if (scored.Count > 1)
                    scored.Sort(s_scoredComparison);

                int take = targetNum < scored.Count ? targetNum : scored.Count;
                if (results.Capacity < take)
                    results.Capacity = take;
                for (int i = 0; i < take; i++)
                    results.Add(scored[i].Value);
            }
        }
    }
}