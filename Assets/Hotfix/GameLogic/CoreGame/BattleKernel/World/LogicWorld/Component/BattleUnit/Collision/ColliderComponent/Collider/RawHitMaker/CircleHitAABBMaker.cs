// using UnityEngine;
//
// namespace LccHotfix
// {
//     public class CircleHitAABBMaker : HitMaker, IRawHitMaker
//     {
//         internal float _radius;
//
//         public void SetRadius(float radius)
//         {
//             _radius = radius;
//         }
//
//         public virtual bool MakeRawHits(LogicEntity ownerEntity, float dt)
//         {
//             var group = ownerEntity.OwnerWorld.GetLogicGroup_Bounds_NoneSubobject();
//             var entities = group.GetEntities();
//             Vector3 position = ownerEntity.comTransform.position;
//             var plane = ownerEntity.OwnerWorld.GetCreationInfo<BattleKernelCreationInfo>().BattlePlane;
//             for (int i = 0; i < entities.Length; i++)
//             {
//                 var entity = entities[i];
//                 if (entity == ownerEntity)
//                     continue;
//
//                 var comBounds = entity.comBounds;
//                 var bounds = comBounds.GetBounds();
//
//                 // 计算小球到障碍物的最近点
//                 var closest = ClosestPoint(position, bounds, plane);
//
//                 // 计算距离和方向
//                 Vector3 dir = position - closest;
//                 float distance = dir.magnitude;
//
//                 // 发生碰撞
//                 if (distance < _radius)
//                 {
//                     RawHits[_hitCount] = new RawHit(entity, closest);
//                     _hitCount++;
//                 }
//
//                 if (_hitCount >= RawHits.Length)
//                 {
//                     break;
//                 }
//             }
//
//             return _hitCount > 0;
//         }
//
//         public override bool IsHit(LogicEntity ownerEntity, float dt, LogicEntity entity, out RawHit hit)
//         {
//             Vector3 position = ownerEntity.comTransform.position;
//             var comBounds = entity.comBounds;
//             var bounds = comBounds.GetBounds();
//             var plane = ownerEntity.OwnerWorld.GetCreationInfo<BattleKernelCreationInfo>().BattlePlane;
//
//             // 计算小球到障碍物的最近点
//             var closest = ClosestPoint(position, bounds, plane);
//
//             // 计算距离和方向
//             Vector3 dir = position - closest;
//             float distance = dir.magnitude;
//             hit = default;
//             bool isHit = false;
//             // 发生碰撞
//             if (distance < _radius)
//             {
//                 isHit = true;
//                 hit = new RawHit(entity, closest);
//             }
//
//             return isHit;
//         }
//
//         private static Vector3 ClosestPoint(Vector3 position, AABB bounds, BattlePlane plane)
//         {
//             var planePoint = AABB.ToPlanePoint(position, plane);
//             var closestPlanePoint = new Vector2(
//                 Mathf.Clamp(planePoint.x, bounds.minPoint.x, bounds.maxPoint.x),
//                 Mathf.Clamp(planePoint.y, bounds.minPoint.y, bounds.maxPoint.y));
//             var fixedAxis = plane == BattlePlane.XY ? position.z : position.y;
//             return AABB.ToWorldPoint(closestPlanePoint, fixedAxis, plane);
//         }
//     }
// }
