// using System;
// using System.Collections.Generic;
// using Vector3 = UnityEngine.Vector3;
//
// namespace LccHotfix
// {
//     /// <summary>
//     /// 己方Raycast，对方AABB
//     /// </summary>
//     public class RaycastHitAABBMaker : HitMaker, IRawHitMaker
//     {
//         public float Distance { get; set; } = 2f;
//
//         public virtual void SetDistance(float distance)
//         {
//             Distance = distance;
//         }
//
//         public virtual bool MakeRawHits(LogicEntity ownerEntity, float dt)
//         {
//             var group = ownerEntity.OwnerWorld.GetLogicGroup_Bounds_NoneSubobject();
//             var entities = group.GetEntities();
//             Vector3 position = ownerEntity.comTransform.position;
//             var plane = ownerEntity.OwnerWorld.GetCreationInfo<BattleKernelCreationInfo>().BattlePlane;
//             if (!TryGetRaycastPlaneDirection(ownerEntity, plane, out var planeDir))
//             {
//                 return false;
//             }
//
//             Vector3 dir = planeDir * Distance * dt / 0.016f;
//             for (int i = 0; i < entities.Length; i++)
//             {
//                 var item = entities[i];
//                 if (item == ownerEntity)
//                     continue;
//
//                 var comBounds = item.comBounds;
//                 var bounds = comBounds.GetBounds();
//                 var begin = position - dir;
//                 var end = position;
//                 if (AABB.Intersect(bounds, begin, end, plane, out var intersectionPoint))
//                 {
//                     RawHits[_hitCount] = new RawHit(item, intersectionPoint);
//                     _hitCount++;
//                     if (_hitCount >= RawHits.Length)
//                     {
//                         break;
//                     }
//                 }
//             }
//
//
//             SortRawHits(dir, plane);
//
//             return _hitCount > 0;
//         }
//
//         private static Comparer<RawHit> _rawHitComparerX1 = Comparer<RawHit>.Create((a, b) => a.Point.x.CompareTo(b.Point.x));
//         private static Comparer<RawHit> _rawHitComparerX2 = Comparer<RawHit>.Create((a, b) => -a.Point.x.CompareTo(b.Point.x));
//         private static Comparer<RawHit> _rawHitComparerY1 = Comparer<RawHit>.Create((a, b) => a.Point.y.CompareTo(b.Point.y));
//         private static Comparer<RawHit> _rawHitComparerY2 = Comparer<RawHit>.Create((a, b) => -a.Point.y.CompareTo(b.Point.y));
//         private static Comparer<RawHit> _rawHitComparerZ1 = Comparer<RawHit>.Create((a, b) => a.Point.z.CompareTo(b.Point.z));
//         private static Comparer<RawHit> _rawHitComparerZ2 = Comparer<RawHit>.Create((a, b) => -a.Point.z.CompareTo(b.Point.z));
//
//         /// <summary>
//         /// 点集按一个方向做逻辑排序
//         /// 保证这些点都在一条直线上，所以做一些小trick，直接按x或y中的某一坐标排序
//         /// </summary>
//         /// <param name="dir">方向</param>
//         public virtual void SortRawHits(Vector3 dir, BattlePlane plane)
//         {
//             var secondAxis = plane == BattlePlane.XY ? dir.y : dir.z;
//             if (Math.Abs(dir.x) > Math.Abs(secondAxis))
//             {
//                 if (dir.x > 0)
//                 {
//                     Array.Sort(RawHits, 0, _hitCount, _rawHitComparerX1);
//                 }
//                 else
//                 {
//                     Array.Sort(RawHits, 0, _hitCount, _rawHitComparerX2);
//                 }
//             }
//             else
//             {
//                 if (plane == BattlePlane.XY)
//                 {
//                     Array.Sort(RawHits, 0, _hitCount, secondAxis > 0 ? _rawHitComparerY1 : _rawHitComparerY2);
//                 }
//                 else
//                 {
//                     Array.Sort(RawHits, 0, _hitCount, secondAxis > 0 ? _rawHitComparerZ1 : _rawHitComparerZ2);
//                 }
//             }
//         }
//
//
//         public override bool IsHit(LogicEntity ownerEntity, float dt, LogicEntity item, out RawHit hit)
//         {
//             bool ishit = false;
//             hit = default;
//             var plane = ownerEntity.OwnerWorld.GetCreationInfo<BattleKernelCreationInfo>().BattlePlane;
//             if (!TryGetRaycastPlaneDirection(ownerEntity, plane, out var planeDir))
//             {
//                 return false;
//             }
//
//             Vector3 dir = planeDir * Distance * dt / 0.016f;
//             Vector3 position = ownerEntity.comTransform.position;
//             var comBounds = item.comBounds;
//             var bounds = comBounds.GetBounds();
//             var begin = position - dir;
//             var end = position;
//             if (AABB.Intersect(bounds, begin, end, plane, out var intersectionPoint))
//             {
//                 hit = new RawHit(item, intersectionPoint);
//                 ishit = true;
//             }
//
//             return ishit;
//         }
//
//         public static bool TryGetRaycastPlaneDirection(LogicEntity ownerEntity, BattlePlane plane, out Vector3 planeDir)
//         {
//             var forward = ownerEntity.comTransform.rotation * (plane == BattlePlane.XY ? Vector3.right : Vector3.forward);
//             return AABB.TryGetNormalizedPlaneDirection(forward, plane, out planeDir);
//         }
//     }
// }
