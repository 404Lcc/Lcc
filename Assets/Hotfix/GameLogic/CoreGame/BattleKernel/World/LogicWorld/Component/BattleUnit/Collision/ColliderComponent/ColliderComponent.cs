using PBConfig;
using UnityEngine;

namespace LccHotfix
{
    public interface IRawHitMaker : IReference
    {
        //生数据
        RawHit[] RawHits { get; }

        //初始化
        void SetCapacity(int capacity);

        //生成生数据 优化后无用，将会移除
        bool MakeRawHits(LogicEntity ownerEntity, float dt);

        //清理当前帧数据
        void Cleanup();
    }

    /// <summary>
    /// 碰撞接口
    /// </summary>
    public interface IEntityColliderHandler : IReference
    {
        IRawHitMaker RawHitMaker { get; }

        void SetRawHitMaker<T>() where T : class, IRawHitMaker, new();

        //生成生数据
        bool CheckRawHits(LogicEntity ownerEntity, float dt);

        //处理生数据
        void HandleRawHits(LogicEntity ownerEntity, RawHit[] rawHits, float dt);

        //清理当前帧数据
        void Cleanup();

        bool IsActiveAsSource(LogicEntity ownerEntity);

        //处理碰撞
        void HandleHitEntity(LogicEntity ownerEntity, LogicEntity hitEntity, RawHit rawHit);
    }

    public class ColliderComponent : LogicComponent
    {
        public bool isActive { get; set; } = true;
        public IEntityColliderHandler handler { get; set; }

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();
            isActive = true;
            if (handler != null)
            {
                ReferencePool.Release(handler);
                handler = null;
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }

    }


    public partial class LogicEntity
    {
        public ColliderComponent comCollider
        {
            get { return (ColliderComponent)GetComponent(LogicComponentsLookup.ComCollider); }
        }

        public bool hasComCollider
        {
            get { return HasComponent(LogicComponentsLookup.ComCollider); }
        }

        public void AddSubobjectComCollider<T, THelper>(TSubobject config, out THelper helper) where T : SubobjectColliderHandlerBase, new() where THelper : class, IRawHitMaker, new()
        {
            var handler = ReferencePool.Acquire<T>();
            handler.SetRawHitMaker<THelper>();
            helper = handler.RawHitMaker as THelper;
            handler.Init(config);
            ReplaceComCollider(handler);
        }

        public T GetSubobjectRawHitHelper<T>() where T : class, IRawHitMaker, new()
        {
            if (!hasComCollider)
                return null;
            var helper = comCollider.handler.RawHitMaker as T;
            if (helper == null)
                return null;
            return helper;
        }

        private void ReplaceComCollider(IEntityColliderHandler handler)
        {
            var index = LogicComponentsLookup.ComCollider;
            var component = (ColliderComponent)CreateComponent(index, typeof(ColliderComponent));
            component.handler = handler;
            ReplaceComponent(index, component);
        }

        public void RemoveComCollider()
        {
            RemoveComponent(LogicComponentsLookup.ComCollider);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComColliderIndex = new ComponentTypeIndex(typeof(ColliderComponent));
        public static int ComCollider => _ComColliderIndex.Index;
    }
}
