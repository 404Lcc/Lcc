using UnityEngine;

namespace LccHotfix
{
    public class BoundsComponent : LogicComponent
    {
        private Vector2 _min;
        private Vector2 _max;
        private AABB _bounds;
        private float _radius;

        public override void PostInitialize(LogicEntity owner)
        {
            base.PostInitialize(owner);

            _owner?.OwnerWorld?.GizmoService?.AddGizmo(OnGizmos);
        }

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();

            _owner?.OwnerWorld?.GizmoService?.RemoveGizmo(OnGizmos);
        }

        public void Init(Vector3 pos, float radius)
        {
            _radius = radius;
            Vector2 halfSize = new Vector2(radius, radius);
            _min = -halfSize;
            _max = halfSize;
            _bounds = new AABB(_min, _max);
            UpdateBounds(pos);
        }

        public void Init(Vector3 pos, Vector2 min, Vector2 max)
        {
            _min = min;
            _max = max;
            _bounds = new AABB(min, max);
            UpdateBounds(pos);
        }

        public virtual void UpdateBounds(Vector3 pos)
        {
            var selfPos = new Vector2(pos.x, pos.y);

            _bounds.minPoint = selfPos + _min;
            _bounds.maxPoint = selfPos + _max;
        }

        public virtual void OnGizmos()
        {
            if (_bounds == null)
                return;
            // 带 Collider 时由 ColliderComponent 绘制同一套 AABB，避免重复
            if (_owner != null && _owner.hasComCollider)
                return;

            _bounds.DrawGizmo(Color.red);
        }

        public AABB GetBounds()
        {
            return _bounds;
        }

        public float GetRadius()
        {
            return _radius;
        }
    }


    public partial class LogicEntity
    {
        public BoundsComponent comBounds
        {
            get { return (BoundsComponent)GetComponent(LogicComponentsLookup.ComBounds); }
        }

        public bool hasComBounds
        {
            get { return HasComponent(LogicComponentsLookup.ComBounds); }
        }

        public void ReplaceComBounds(Vector3 pos, float radius)
        {
            var index = LogicComponentsLookup.ComBounds;
            var component = (BoundsComponent)CreateComponent(index, typeof(BoundsComponent));
            component.Init(pos, radius);
            ReplaceComponent(index, component);
        }
        
        public void ReplaceComBounds(Vector3 pos, Vector2 min, Vector2 max)
        {
            var index = LogicComponentsLookup.ComBounds;
            var component = (BoundsComponent)CreateComponent(index, typeof(BoundsComponent));
            component.Init(pos, min, max);
            ReplaceComponent(index, component);
        }

        public void RemoveComBounds()
        {
            RemoveComponent(LogicComponentsLookup.ComBounds);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComBoundsIndex = new ComponentTypeIndex(typeof(BoundsComponent));
        public static int ComBounds => _ComBoundsIndex.Index;
    }
}
