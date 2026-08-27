using UnityEngine;

namespace LccHotfix
{
    public class HitMakerLogicPhysics : HitMaker, IRawHitMaker
    {
        public virtual bool MakeRawHits(LogicEntity ownerEntity, float dt)
        {
            KLogger.LogError("这个 HitMakerLogicPhysics 已经被废弃了，不该被用到");
            var group = ownerEntity.OwnerWorld.GetLogicGroup_Bounds_NoneSubobject();
            var entities = group.GetEntities();
            for (int i = 0; i < entities.Length; i++)
            {
                if (IsFull())
                {
                    break;
                }

                if (IsHit(ownerEntity, dt, entities[i], out var hit))
                    Add(hit);
            }

            return ContainsHit();
        }

        public override bool IsHit(LogicEntity ownerEntity, float dt, LogicEntity item, out RawHit hit)
        {
            hit = default;

            if (!ownerEntity.hasComBounds || item == ownerEntity)
                return false;

            var ownerBounds = ownerEntity.comBounds.GetBounds();
            var targetBounds = item.comBounds.GetBounds();
            if (ownerBounds == null || targetBounds == null || !ownerBounds.Collision(targetBounds))
                return false;

            var plane = ownerEntity.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.BattlePlane ?? BattlePlane.XZ;
            var normal = EstimateHitNormal(ownerBounds, targetBounds, plane, ownerEntity);
            var hitPoint = EstimateHitPoint(ownerBounds, targetBounds, plane, ownerEntity.comTransform.position);
            hit = new RawHit(item, hitPoint, normal);
            return true;
        }

        // 按最小穿透轴估算法线，比中心差在深度重叠时更稳。
        private static Vector3 EstimateHitNormal(AABB ownerBounds, AABB targetBounds, BattlePlane plane, LogicEntity ownerEntity)
        {
            var overlapX = Mathf.Min(ownerBounds.maxPoint.x, targetBounds.maxPoint.x) - Mathf.Max(ownerBounds.minPoint.x, targetBounds.minPoint.x);
            var overlapY = Mathf.Min(ownerBounds.maxPoint.y, targetBounds.maxPoint.y) - Mathf.Max(ownerBounds.minPoint.y, targetBounds.minPoint.y);

            var ownerCenter = (ownerBounds.minPoint + ownerBounds.maxPoint) * 0.5f;
            var targetCenter = (targetBounds.minPoint + targetBounds.maxPoint) * 0.5f;
            var delta = ownerCenter - targetCenter;

            Vector2 n2;
            if (overlapX <= overlapY)
            {
                var sign = delta.x >= 0f ? 1f : -1f;
                if (Mathf.Abs(delta.x) <= 0.0001f)
                    sign = PreferSignFromFacing(ownerEntity, plane, axisX: true);
                n2 = new Vector2(sign, 0f);
            }
            else
            {
                var sign = delta.y >= 0f ? 1f : -1f;
                if (Mathf.Abs(delta.y) <= 0.0001f)
                {
                    sign = PreferSignFromFacing(ownerEntity, plane, axisX: false);
                }

                n2 = new Vector2(0f, sign);
            }

            return plane == BattlePlane.XY
                ? new Vector3(n2.x, n2.y, 0f)
                : new Vector3(n2.x, 0f, n2.y);
        }

        private static float PreferSignFromFacing(LogicEntity ownerEntity, BattlePlane plane, bool axisX)
        {
            var facing = ownerEntity.comTransform.rotation * Vector3.forward;
            var p = AABB.ToPlanePoint(facing, plane);
            var v = axisX ? p.x : p.y;
            // 外法线应与入射方向相对。
            if (Mathf.Abs(v) <= 0.0001f)
            {
                return 1f;
            }

            return v > 0f ? -1f : 1f;
        }

        private static Vector3 EstimateHitPoint(AABB ownerBounds, AABB targetBounds, BattlePlane plane, Vector3 fallbackWorld)
        {
            var minX = Mathf.Max(ownerBounds.minPoint.x, targetBounds.minPoint.x);
            var maxX = Mathf.Min(ownerBounds.maxPoint.x, targetBounds.maxPoint.x);
            var minY = Mathf.Max(ownerBounds.minPoint.y, targetBounds.minPoint.y);
            var maxY = Mathf.Min(ownerBounds.maxPoint.y, targetBounds.maxPoint.y);
            var mid = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            var fixedAxis = plane == BattlePlane.XY ? fallbackWorld.z : fallbackWorld.y;
            return AABB.ToWorldPoint(mid, fixedAxis, plane);
        }
    }
}