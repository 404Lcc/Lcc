using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class SysCollision : IExecuteSystem
    {
        private readonly BattleKernelCreationInfo creationInfo;
        private readonly LogicWorld logicWorld;
        private readonly IGroup<LogicEntity> enemyGroup;
        private readonly IGroup<LogicEntity> friendGroup;
        private AQuadSpace root;
        private int rootConfigVersion;
        private readonly CollectResult result;
        private int statFrame;
        private int statSourceCount;
        private int statTargetCount;
        private int statCandidateCount;
        private int statRawHitCount;
        private int statOutOfRootCount;

        public SysCollision(ECWorlds world)
        {
            creationInfo = world.GetCreationInfo<BattleKernelCreationInfo>();
            logicWorld = world.LogicWorld;
            enemyGroup = logicWorld.GetLogicGroup_Bounds_NoneSubobject();
            friendGroup = logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComCollider, LogicComponentsLookup.ComTransform, LogicComponentsLookup.ComBounds));
            result = new CollectResult();
            RebuildSpaceIfNeeded(true);
        }

        public void Execute()
        {
            RebuildSpaceIfNeeded(false);
            var dt = BattleTime.GetDeltaTime(logicWorld);
            var enemies = enemyGroup.GetEntities();
            var friends = GetEntities(dt);
            FilterHit(friends, enemies, dt);
            Handle(friends, dt);
            LogStatsIfNeeded();
        }

        private void RebuildSpaceIfNeeded(bool force)
        {
            var config = creationInfo?.CollisionSpaceConfig;
            if (config == null)
            {
                return;
            }

            if (!force && root != null && rootConfigVersion == config.Version)
            {
                return;
            }

            root = SpatialPartition.Create(config.FullSpace, config.QuadTreeDepth);
            rootConfigVersion = config.Version;
            if (BattleLogger.IsDebugEnabled)
            {
                var aabb = config.FullSpace;
                BattleLogger.LogDebug($"SysCollision rebuild space min=({aabb.minPoint.x:F2},{aabb.minPoint.y:F2}) max=({aabb.maxPoint.x:F2},{aabb.maxPoint.y:F2}) depth={config.QuadTreeDepth} version={config.Version}");
            }
        }

        private void FilterHit(List<LogicEntity> friends, LogicEntity[] enemies, float dt)
        {
            ResetStats(friends.Count, enemies.Length);
            root.Clear();
            result.result.Clear();
            root.AddObjects(ToSpace(friends, 0));
            root.AddObjects(ToSpace(enemies, 1));
            root.Collect(result);
            statCandidateCount = result.result.Count;
            foreach (var item in result.result)
            {
                if (!TryFindInfo(item, out var friend, out var enemy))
                {
                    continue;
                }

                var maker = friend.comCollider.handler.RawHitMaker;
                if (maker is HitMaker hm)
                {
                    if (hm.IsFull())
                    {
                        continue;
                    }

                    if (hm.IsHit(friend, dt, enemy, out var hit))
                    {
                        hm.Add(hit);
                        statRawHitCount++;
                    }
                }
            }
        }

        private bool TryFindInfo(CollectPair item, out LogicEntity friend, out LogicEntity enemy)
        {
            friend = item.obj0 as LogicEntity;
            enemy = item.obj1 as LogicEntity;
            if (friend == null || enemy == null)
            {
                return false;
            }

            if (enemy.hasComCollider)
            {
                var tem = friend;
                friend = enemy;
                enemy = tem;
            }

            return friend.hasComCollider && enemy.hasComBounds;
        }

        private void Handle(List<LogicEntity> friends, float dt)
        {
            foreach (var entity in friends)
            {
                var handler = entity.comCollider.handler;
                var maker = handler.RawHitMaker;
                bool hasHits = CheckRawHits(dt, entity, maker);
                if (hasHits)
                {
                    handler.HandleRawHits(entity, handler.RawHitMaker.RawHits, dt);
                }

                handler.Cleanup();
            }
        }

        private IEnumerable<SpaceObject> ToSpace(IEnumerable<LogicEntity> v, int layer)
        {
            foreach (var item in v)
            {
                if (item == null || !item.hasComBounds)
                {
                    continue;
                }

                var aabb = item.comBounds.GetBounds();
                if (aabb == null)
                {
                    continue;
                }

                if (root?.aabb != null && !root.aabb.Intersects(aabb))
                {
                    statOutOfRootCount++;
                }

                yield return new SpaceObject(aabb, item, layer);
            }
        }

        private List<LogicEntity> GetEntities(float dt)
        {
            var entities = new List<LogicEntity>();
            foreach (var entity in friendGroup.GetEntities())
            {
                if (CanCollision(entity, dt))
                {
                    entities.Add(entity);
                }
            }

            return entities;
        }

        private bool CheckRawHits(float dt, LogicEntity ownerEntity, IRawHitMaker maker)
        {
            if (maker is RaycastHitAABBMaker ray1)
            {
                Vector3 dir1 = ownerEntity.comTransform.rotation * Vector3.right * ray1.Distance * dt / 0.016f;
                ray1.SortRawHits(dir1);
            }

            if (maker is HitMaker hm1)
            {
                return hm1.ContainsHit();
            }

            return false;
        }

        private bool CanCollision(LogicEntity entity, float dt)
        {
            if (entity == null)
                return false;
            if (entity.hasComAttributes)
            {
                return entity.comAttributes.GetValue<bool>(AttributeBool.CanCollision);
            }

            var handler = entity.comCollider.handler;
            if (handler.GetType() == typeof(SubobjectColliderHandlerBase))
            {
                var ch = handler as SubobjectColliderHandlerBase;
                //主动碰撞间隔
                if (ch._hitIntervalTimer > 0)
                {
                    ch._hitIntervalTimer -= dt;
                }

                if (!ch.IsActiveAsSource(entity))
                    return false;
            }

            return true;
        }

        private void ResetStats(int sourceCount, int targetCount)
        {
            statSourceCount = sourceCount;
            statTargetCount = targetCount;
            statCandidateCount = 0;
            statRawHitCount = 0;
            statOutOfRootCount = 0;
        }

        private void LogStatsIfNeeded()
        {
            if (creationInfo?.CollisionSpaceConfig == null || !creationInfo.CollisionSpaceConfig.EnableStatsLog)
            {
                return;
            }

            if (!BattleLogger.IsDebugEnabled)
            {
                return;
            }

            statFrame++;
            if (statFrame < 60)
            {
                return;
            }

            statFrame = 0;
            BattleLogger.LogDebug($"SysCollision stats source={statSourceCount}, target={statTargetCount}, candidate={statCandidateCount}, rawHit={statRawHitCount}, outOfRoot={statOutOfRootCount}");
        }
    }
}
