// using Entitas;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace LccHotfix
// {
//     public class SysAABBCollision : IExecuteSystem, ITearDownSystem
//     {
//         private readonly BattleKernelCreationInfo creationInfo;
//         private readonly LogicWorld logicWorld;
//         private readonly IGroup<LogicEntity> enemyGroup;
//         private readonly IGroup<LogicEntity> friendGroup;
//         private AQuadSpace root;
//         private int rootConfigVersion;
//         private readonly CollectResult result;
//         private int statFrame;
//         private int statSourceCount;
//         private int statTargetCount;
//         private int statCandidateCount;
//         private int statRawHitCount;
//         private int statOutOfRootCount;
//
//         private System.Action _gizmoAction;
//
//         public SysAABBCollision(ECWorlds world)
//         {
//             creationInfo = world.GetCreationInfo<BattleKernelCreationInfo>();
//             logicWorld = world.LogicWorld;
//             enemyGroup = logicWorld.GetLogicGroup_Bounds_NoneSubobject();
//             friendGroup = logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComCollider, LogicComponentsLookup.ComTransform, LogicComponentsLookup.ComBounds));
//             result = new CollectResult();
//             RebuildSpaceIfNeeded(true);
//
//             _gizmoAction = OnDrawGizmos;
//             creationInfo?.GizmoService?.AddGizmo(_gizmoAction);
//         }
//
//         public void TearDown()
//         {
//             creationInfo?.GizmoService?.RemoveGizmo(_gizmoAction);
//         }
//
//         private void OnDrawGizmos()
//         {
//             var space = creationInfo?.CollisionSpaceConfig?.FullSpace;
//             if (space == null)
//                 return;
//             var plane = creationInfo.BattlePlane;
//             space.DrawGizmo(plane, Color.magenta);
//         }
//
//         public void Execute()
//         {
//             RebuildSpaceIfNeeded(false);
//             var dt = BattleTime.GetDeltaTime(logicWorld);
//             var enemies = enemyGroup.GetEntities();
//             var friends = GetEntities(dt);
//             ExpandCollisionSpace(friends, enemies);
//             RebuildSpaceIfNeeded(false);
//             FilterHit(friends, enemies, dt);
//             Handle(friends, dt);
//             LogStatsIfNeeded();
//         }
//
//         private void ExpandCollisionSpace(List<LogicEntity> friends, LogicEntity[] enemies)
//         {
//             float minX = 0, maxX = 0, minY = 0, maxY = 0;
//             bool hasAny = false;
//             System.Action<LogicEntity> collect = (e) =>
//             {
//                 if (e != null && e.hasComTransform)
//                 {
//                     var p = AABB.ToPlanePoint(e.comTransform.position, creationInfo.BattlePlane);
//                     if (!hasAny)
//                     {
//                         minX = maxX = p.x;
//                         minY = maxY = p.y;
//                         hasAny = true;
//                     }
//                     else
//                     {
//                         if (p.x < minX) minX = p.x;
//                         if (p.x > maxX) maxX = p.x;
//                         if (p.y < minY) minY = p.y;
//                         if (p.y > maxY) maxY = p.y;
//                     }
//                 }
//             };
//             foreach (var e in friends) collect(e);
//             foreach (var e in enemies) collect(e);
//             if (hasAny)
//             {
//                 var extra = 5f;
//                 creationInfo?.CollisionSpaceConfig?.SetFullSpace(
//                     new AABB(new Vector2(minX - extra, minY - extra), new Vector2(maxX + extra, maxY + extra)));
//             }
//         }
//
//         private void RebuildSpaceIfNeeded(bool force)
//         {
//             var config = creationInfo?.CollisionSpaceConfig;
//             if (config == null)
//             {
//                 return;
//             }
//
//             if (!force && root != null && rootConfigVersion == config.Version)
//             {
//                 return;
//             }
//
//             root = SpatialPartition.Create(config.FullSpace, config.QuadTreeDepth);
//             rootConfigVersion = config.Version;
//             if (BattleLogger.IsDebugEnabled)
//             {
//                 var aabb = config.FullSpace;
//                 BattleLogger.LogDebug($"SysAABBCollision rebuild space min=({aabb.minPoint.x:F2},{aabb.minPoint.y:F2}) max=({aabb.maxPoint.x:F2},{aabb.maxPoint.y:F2}) depth={config.QuadTreeDepth} version={config.Version}");
//             }
//         }
//
//         private void FilterHit(List<LogicEntity> friends, LogicEntity[] enemies, float dt)
//         {
//             ResetStats(friends.Count, enemies.Length);
//             root.Clear();
//             result.result.Clear();
//             root.AddObjects(ToSpace(friends, 0));
//             root.AddObjects(ToSpace(enemies, 1));
//             root.Collect(result);
//             statCandidateCount = result.result.Count;
//             foreach (var item in result.result)
//             {
//                 if (!TryFindInfo(item, out var friend, out var enemy))
//                 {
//                     continue;
//                 }
//
//                 var maker = friend.comCollider.handler.RawHitMaker;
//                 if (maker is HitMaker hm)
//                 {
//                     if (hm.IsFull())
//                     {
//                         continue;
//                     }
//
//                     if (hm.IsHit(friend, dt, enemy, out var hit))
//                     {
//                         hm.Add(hit);
//                         statRawHitCount++;
//                     }
//                 }
//             }
//         }
//
//         private bool TryFindInfo(CollectPair item, out LogicEntity friend, out LogicEntity enemy)
//         {
//             friend = item.obj0 as LogicEntity;
//             enemy = item.obj1 as LogicEntity;
//             if (friend == null || enemy == null)
//             {
//                 return false;
//             }
//
//             if (enemy.hasComCollider)
//             {
//                 var tem = friend;
//                 friend = enemy;
//                 enemy = tem;
//             }
//
//             return friend.hasComCollider && enemy.hasComBounds;
//         }
//
//         private void Handle(List<LogicEntity> friends, float dt)
//         {
//             foreach (var entity in friends)
//             {
//                 if (!entity.comCollider.isActive)
//                 {
//                     continue;
//                 }
//
//                 var handler = entity.comCollider.handler;
//                 var maker = handler.RawHitMaker;
//                 bool hasHits = CheckRawHits(dt, entity, maker);
//                 if (hasHits)
//                 {
//                     handler.HandleRawHits(entity, handler.RawHitMaker.RawHits, dt);
//                 }
//
//                 handler.Cleanup();
//             }
//         }
//
//         private IEnumerable<SpaceObject> ToSpace(IEnumerable<LogicEntity> v, int layer)
//         {
//             foreach (var item in v)
//             {
//                 if (item == null || !item.hasComBounds)
//                 {
//                     continue;
//                 }
//
//                 var aabb = item.comBounds.GetBounds();
//                 if (aabb == null)
//                 {
//                     continue;
//                 }
//
//                 if (root?.aabb != null && !root.aabb.Intersects(aabb))
//                 {
//                     statOutOfRootCount++;
//                 }
//
//                 yield return new SpaceObject(aabb, item, layer);
//             }
//         }
//
//         private List<LogicEntity> GetEntities(float dt)
//         {
//             var entities = new List<LogicEntity>();
//             foreach (var entity in friendGroup.GetEntities())
//             {
//                 if (entity.comCollider.isActive && CanCollision(entity, dt))
//                 {
//                     entities.Add(entity);
//                 }
//             }
//
//             return entities;
//         }
//
//         private bool CheckRawHits(float dt, LogicEntity ownerEntity, IRawHitMaker maker)
//         {
//             if (maker is RaycastHitAABBMaker ray1)
//             {
//                 if (!RaycastHitAABBMaker.TryGetRaycastPlaneDirection(ownerEntity, creationInfo.BattlePlane, out var planeDir))
//                 {
//                     return false;
//                 }
//
//                 Vector3 dir1 = planeDir * ray1.Distance * dt / 0.016f;
//                 ray1.SortRawHits(dir1, creationInfo.BattlePlane);
//             }
//
//             if (maker is HitMaker hm1)
//             {
//                 return hm1.ContainsHit();
//             }
//
//             return false;
//         }
//
//         private bool CanCollision(LogicEntity entity, float dt)
//         {
//             if (entity == null)
//                 return false;
//             if (entity.hasComAttributes && !entity.comAttributes.GetValue<bool>(AttributeBool.CanCollision))
//             {
//                 return false;
//             }
//
//             var handler = entity.comCollider.handler;
//             if (handler is SubobjectColliderHandlerBase ch)
//             {
//                 //主动碰撞间隔
//                 if (ch._hitIntervalTimer > 0)
//                 {
//                     ch._hitIntervalTimer -= dt;
//                 }
//
//                 if (!handler.IsActiveAsSource(entity))
//                     return false;
//             }
//
//             return true;
//         }
//
//         private void ResetStats(int sourceCount, int targetCount)
//         {
//             statSourceCount = sourceCount;
//             statTargetCount = targetCount;
//             statCandidateCount = 0;
//             statRawHitCount = 0;
//             statOutOfRootCount = 0;
//         }
//
//         private void LogStatsIfNeeded()
//         {
//             if (creationInfo?.CollisionSpaceConfig == null || !creationInfo.CollisionSpaceConfig.EnableStatsLog)
//             {
//                 return;
//             }
//
//             if (!BattleLogger.IsDebugEnabled)
//             {
//                 return;
//             }
//
//             statFrame++;
//             if (statFrame < 60)
//             {
//                 return;
//             }
//
//             statFrame = 0;
//             BattleLogger.LogDebug($"SysAABBCollision stats source={statSourceCount}, target={statTargetCount}, candidate={statCandidateCount}, rawHit={statRawHitCount}, outOfRoot={statOutOfRootCount}");
//         }
//     }
// }
