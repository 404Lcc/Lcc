using Vector3 = UnityEngine.Vector3;

namespace LccHotfix
{
    public struct RawHit
    {
        public LogicEntity HitEntity { get; set; }
        public Vector3 Point { get; set; }
        public Vector3 Normal { get; set; }
        // Unity layer index；0 表示未设置（逻辑碰撞路径）。
        public int ColliderLayer { get; set; }

        public RawHit(LogicEntity hitEntity, Vector3 point, Vector3 normal = default, int colliderLayer = 0)
        {
            HitEntity = hitEntity;
            Point = point;
            Normal = normal;
            ColliderLayer = colliderLayer;
        }
    }

    public abstract class ColliderHandler : IEntityColliderHandler
    {
        public IRawHitMaker RawHitMaker { get; private set; }

        public virtual void SetRawHitMaker<T>() where T : class, IRawHitMaker, new()
        {
            if (RawHitMaker != null)
            {
                UnityEngine.Debug.LogError("已经存在一个GenRawHitHelper了 有问题请检查" + this.GetType().Name);
            }

            var helper = ReferencePool.Acquire<T>();
            helper.SetCapacity(32);
            RawHitMaker = helper;
        }

        public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
        {
            return RawHitMaker.MakeRawHits(ownerEntity, dt);
        }

        public abstract void HandleRawHits(LogicEntity ownerEntity, RawHit[] rawHits, float dt);

        public void Cleanup()
        {
            RawHitMaker.Cleanup();
        }

        public abstract bool IsActiveAsSource(LogicEntity ownerEntity);

        public abstract void HandleHitEntity(LogicEntity ownerEntity, LogicEntity hitEntity, RawHit rawHit);

        public virtual void OnRecycle()
        {
            Cleanup();
            ReferencePool.Release(RawHitMaker);
            RawHitMaker = null;
        }
    }
}