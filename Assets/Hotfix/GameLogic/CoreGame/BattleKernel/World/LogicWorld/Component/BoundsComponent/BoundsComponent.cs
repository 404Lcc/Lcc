using UnityEngine;

namespace LccHotfix
{
    public class BoundsComponent : LogicComponent
    {
        private Vector2 _min;
        private Vector2 _max;
        private AABB _bounds;
        private float _radius;
        private float _modelRadius; // 真实模型半径，NavMeshAgent等Unity组件会自动处理Scale

        public override void PostInitialize(LogicEntity owner)
        {
            base.PostInitialize(owner);

            _owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.GizmoService?.AddGizmo(OnGizmos);
        }

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();

            _owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.GizmoService?.RemoveGizmo(OnGizmos);
        }

        public void Init(Vector3 pos, float radius, float scale, BattlePlane plane)
        {
            _radius = radius * scale;
            _modelRadius = radius;
            Vector2 halfSize = new Vector2(radius, radius);
            _min = -halfSize;
            _max = halfSize;
            _bounds = new AABB(_min, _max);
            UpdateBounds(pos, plane);
        }

        public void Init(Vector3 pos, Vector2 min, Vector2 max, BattlePlane plane)
        {
            _min = min;
            _max = max;
            _bounds = new AABB(min, max);
            UpdateBounds(pos, plane);
        }

        public void UpdateBounds(Vector3 pos)
        {
            var plane = _owner.OwnerWorld.GetCreationInfo<BattleKernelCreationInfo>().BattlePlane;
            UpdateBounds(pos, plane);
        }

        private void UpdateBounds(Vector3 pos, BattlePlane plane)
        {
            var selfPos = AABB.ToPlanePoint(pos, plane);
            _bounds.minPoint = selfPos + _min;
            _bounds.maxPoint = selfPos + _max;
        }

        public void OnGizmos()
        {
            if (_bounds == null || _owner?.OwnerWorld == null)
                return;

            var plane = _owner.OwnerWorld.GetCreationInfo<BattleKernelCreationInfo>().BattlePlane;
            var color = _owner.hasComSubobject ? Color.yellow : Color.cyan;
            _bounds.DrawGizmo(plane, color);
        }

        public AABB GetBounds()
        {
            return _bounds;
        }

        public float GetRadius()
        {
            return _radius;
        }

        public float GetModelRadius()
        {
            return _modelRadius;
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

        public void ReplaceComBounds(Vector3 pos, float radius, float scale = 1f)
        {
            var index = LogicComponentsLookup.ComBounds;
            var component = (BoundsComponent)CreateComponent(index, typeof(BoundsComponent));
            var plane = OwnerWorld.GetCreationInfo<BattleKernelCreationInfo>().BattlePlane;
            component.Init(pos, radius, scale, plane);
            ReplaceComponent(index, component);
        }

        public void ReplaceComBounds(Vector3 pos, Vector2 min, Vector2 max)
        {
            var index = LogicComponentsLookup.ComBounds;
            var component = (BoundsComponent)CreateComponent(index, typeof(BoundsComponent));
            var plane = OwnerWorld.GetCreationInfo<BattleKernelCreationInfo>().BattlePlane;
            component.Init(pos, min, max, plane);
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
