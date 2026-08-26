using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace LccHotfix
{
    /// <summary>
    /// 逻辑圆 vs AABB（配合 SysAABBCollision 使用）
    /// </summary>
    public class CircleAabbHitMaker : HitMaker, IRawHitMaker
    {
        internal float _radius;

        public void SetRadius(float radius)
        {
            _radius = radius;
        }

        public virtual bool MakeRawHits(LogicEntity ownerEntity, float dt)
        {
            var group = ownerEntity.OwnerWorld.GetLogicGroup_Bounds_NoneSubobject();
            var entities = group.GetEntities();
            Vector3 position = ownerEntity.comTransform.position;
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                if (entity == ownerEntity)
                    continue;

                var comBounds = entity.comBounds;
                var bounds = comBounds.GetBounds();

                Vector3 closest = new Vector3(Mathf.Clamp(position.x, bounds.minPoint.x, bounds.maxPoint.x), Mathf.Clamp(position.y, bounds.minPoint.y, bounds.maxPoint.y), position.z);

                Vector3 dir = position - closest;
                float distance = dir.magnitude;

                if (distance < _radius)
                {
                    RawHits[_hitCount] = new RawHit(entity, closest);
                    _hitCount++;
                }

                if (_hitCount >= RawHits.Length)
                {
                    break;
                }
            }

            return _hitCount > 0;
        }

        public override bool IsHit(LogicEntity ownerEntity, float dt, LogicEntity entity, out RawHit hit)
        {
            Vector3 position = ownerEntity.comTransform.position;
            var comBounds = entity.comBounds;
            var bounds = comBounds.GetBounds();

            Vector3 closest = new Vector3(Mathf.Clamp(position.x, bounds.minPoint.x, bounds.maxPoint.x), Mathf.Clamp(position.y, bounds.minPoint.y, bounds.maxPoint.y), position.z);

            Vector3 dir = position - closest;
            float distance = dir.magnitude;
            hit = default;
            bool isHit = false;
            if (distance < _radius)
            {
                isHit = true;
                hit = new RawHit(entity, closest);
            }

            return isHit;
        }
    }
}