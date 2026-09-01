using System;
using UnityEngine;

namespace LccHotfix
{
    public class HitMakerUnityPhysics : HitMaker, IRawHitMaker
    {
        private const float MinCapsuleRadius = 0.01f;
        private const float MinMoveSqr = 0.0000001f;
        private static readonly int DefaultLayerMask = LayerMask.GetMask("Body", "Block", "MonsterBody");
        // 启动时缓存，热路径用 int 比较，避免 NameToLayer / LayerToName 字符串。
        internal static readonly int BlockLayer = LayerMask.NameToLayer("Block");

        private Collider[] overlapHits;
        private RaycastHit[] rayHits;
        private bool hasLastPos;
        private Vector3 lastPos;
        private int layerMask = DefaultLayerMask;

        public override void SetCapacity(int capacity)
        {
            base.SetCapacity(capacity);
            // 容量不足才分配，避免池化取出后打掉已有缓冲。
            if (overlapHits == null || overlapHits.Length < capacity)
                overlapHits = new Collider[capacity];
            if (rayHits == null || rayHits.Length < capacity)
                rayHits = new RaycastHit[capacity];
        }

        public bool MakeRawHits(LogicEntity ownerEntity, float dt)
        {
            var origin = ownerEntity.comTransform.position;
            var isFirstHit = !hasLastPos;
            if (!hasLastPos)
            {
                hasLastPos = true;
                // SysLocomotion 先位移再进碰撞：仅在本帧 DeltaPosition 有效时回推起点。
                if (ownerEntity.hasComLocomotion && ownerEntity.comLocomotion.Locomotion != null)
                {
                    var delta = ownerEntity.comLocomotion.Locomotion.DeltaPosition;
                    lastPos = delta.sqrMagnitude > MinMoveSqr ? origin - delta : origin;
                }
                else
                {
                    lastPos = origin;
                }
            }

            var start = lastPos;
            lastPos = origin;

            var move = origin - start;
            // 对齐 HitMakerLogicPhysics：几何命中全收，阵营/死亡/白名单等业务过滤交给 Handler。
            if (ownerEntity.hasComBounds)
            {
                if (ownerEntity.comBounds.GetRadius() == 0f && move.sqrMagnitude > MinMoveSqr)
                {
                    if (isFirstHit)
                    {
                        var overlapCount = Physics.OverlapSphereNonAlloc(start, 0f, overlapHits, layerMask);
                        for (var i = 0; i < overlapCount && !IsFull(); i++)
                        {
                            if (IsHit(ownerEntity, overlapHits[i], start, origin, out var hit))
                                Add(hit);
                        }
                    }
                    var hitCount = Physics.RaycastNonAlloc(start, move.normalized, rayHits, move.magnitude, layerMask);
                    for (var i = 0; i < hitCount && !IsFull(); i++)
                    {
                        if (IsHit(ownerEntity, rayHits[i].collider, start, origin, out var hit))
                            Add(hit);
                    }
                }
                else
                {
                    // 两端重合时 OverlapCapsule 等价于球检测，覆盖生成即重叠 / 当帧未位移。
                    var radius = Mathf.Max(ownerEntity.comBounds.GetRadius(), MinCapsuleRadius);
                    var hitCount = Physics.OverlapCapsuleNonAlloc(start, origin, radius, overlapHits, layerMask);
                    for (var i = 0; i < hitCount && !IsFull(); i++)
                    {
                        if (IsHit(ownerEntity, overlapHits[i], start, origin, out var hit))
                            Add(hit);
                    }
                }
            }

            // 临时关掉近到远排序：业务对命中顺序不敏感，先观察是否可永久去掉。
            // if (_hitCount > 1)
            //     SortHitsByDistanceFrom(start);

            return ContainsHit();
        }

        // 临时关掉近到远排序时一并闲置；确认业务不需要后再删。
        // private void SortHitsByDistanceFrom(Vector3 start)
        // {
        //     for (var i = 1; i < _hitCount; i++)
        //     {
        //         var current = RawHits[i];
        //         var currentDist = (current.Point - start).sqrMagnitude;
        //         var j = i - 1;
        //         while (j >= 0 && (RawHits[j].Point - start).sqrMagnitude > currentDist)
        //         {
        //             RawHits[j + 1] = RawHits[j];
        //             j--;
        //         }
        //         RawHits[j + 1] = current;
        //     }
        // }

        // 对齐 AABB：IsHit 只做几何判定；射线版入口不按逻辑实体扫，这里保持空实现。
        public override bool IsHit(LogicEntity ownerEntity, float dt, LogicEntity item, out RawHit hit)
        {
            hit = default;
            return false;
        }

        // 对齐 AABB 的 IsHit 职责：只做几何 + 实体反查，不做业务过滤。
        private bool IsHit(LogicEntity ownerEntity, Collider collider, Vector3 start, Vector3 end, out RawHit hit)
        {
            hit = default;
            if (collider == null)
                return false;

            var entity = ownerEntity.OwnerWorld.GetEntitiesWithComUnityObjectRelated(collider.gameObject.GetInstanceID());
            var layer = collider.gameObject.layer;

            // 有实体：自己跳过；无实体：仅 Block 作为障碍保留。
            if (entity == null)
            {
                if (layer != BlockLayer)
                    return false;
            }
            else if (entity == ownerEntity)
            {
                return false;
            }

            EstimateHitPointAndNormal(collider, start, end - start, out var point, out var normal);
            hit = new RawHit(entity, point, normal, layer);
            return true;
        }

        /// <summary>
        /// 外点：ClosestPoint 表面法线。
        /// 内点：bounds 最短脱出轴估法线，避免 ClosestPoint 返回自身后退化成 Vector3.up。
        /// </summary>
        private static void EstimateHitPointAndNormal(Collider collider, Vector3 start, Vector3 moveDir, out Vector3 point, out Vector3 normal)
        {
            point = collider.ClosestPoint(start);
            var toPoint = point - start;
            if (toPoint.sqrMagnitude > MinMoveSqr)
            {
                // start 在外侧：表面法线朝向 start。
                normal = -toPoint.normalized;
                return;
            }

            // start 已深入 Collider：用 world AABB 最短穿透轴估外法线。
            var bounds = collider.bounds;
            var center = bounds.center;
            var extents = bounds.extents;
            var local = start - center;

            var penX = extents.x - Mathf.Abs(local.x);
            var penY = extents.y - Mathf.Abs(local.y);
            var penZ = extents.z - Mathf.Abs(local.z);

            if (penX <= penY && penX <= penZ)
            {
                var sign = ResolveInsideAxisSign(local.x, moveDir.x);
                normal = new Vector3(sign, 0f, 0f);
                point = new Vector3(center.x + sign * extents.x, start.y, start.z);
                return;
            }

            if (penY <= penZ)
            {
                var sign = ResolveInsideAxisSign(local.y, moveDir.y);
                normal = new Vector3(0f, sign, 0f);
                point = new Vector3(start.x, center.y + sign * extents.y, start.z);
                return;
            }

            {
                var sign = ResolveInsideAxisSign(local.z, moveDir.z);
                normal = new Vector3(0f, 0f, sign);
                point = new Vector3(start.x, start.y, center.z + sign * extents.z);
            }
        }

        // 中心重合时按运动反方向选外法线，便于反射。
        private static float ResolveInsideAxisSign(float localAxis, float moveAxis)
        {
            if (Mathf.Abs(localAxis) > 0.0001f)
                return localAxis >= 0f ? 1f : -1f;
            if (Mathf.Abs(moveAxis) > 0.0001f)
                return moveAxis > 0f ? -1f : 1f;
            return 1f;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            if (overlapHits != null)
                Array.Clear(overlapHits, 0, overlapHits.Length);
            if (rayHits != null)
                Array.Clear(rayHits, 0, rayHits.Length);
        }

        public override void OnRecycle()
        {
            Cleanup();
            hasLastPos = false;
            lastPos = default;
            layerMask = DefaultLayerMask;
        }
    }
}
