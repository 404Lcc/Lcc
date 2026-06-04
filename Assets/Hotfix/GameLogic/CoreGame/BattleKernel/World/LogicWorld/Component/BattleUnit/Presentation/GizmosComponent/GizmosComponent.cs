using System.Collections.Generic;

namespace LccHotfix
{
    public delegate void EntityGizmoAction(LogicEntity entity);

    public class GizmosComponent : LogicComponent
    {
        private readonly Dictionary<string, EntityGizmoAction> _gizmos = new();

        public bool IsEmpty => _gizmos.Count == 0;

        public override void PostInitialize(LogicEntity owner)
        {
            base.PostInitialize(owner);

            _owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.GizmoService?.AddGizmo(OnGizmos);
        }

        public override void DisposeOnRemove()
        {
            _owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.GizmoService?.RemoveGizmo(OnGizmos);
            _gizmos.Clear();

            base.DisposeOnRemove();
        }

        public void AddGizmo(string name, EntityGizmoAction action)
        {
            if (string.IsNullOrEmpty(name) || action == null)
                return;

            _gizmos[name] = action;
            if (_owner != null)
            {
                _owner.ReplaceComponent(LogicComponentsLookup.ComGizmos, this);
            }
        }

        public void RemoveGizmo(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            _gizmos.Remove(name);
            if (_owner == null)
                return;

            if (_gizmos.Count == 0)
            {
                _owner.RemoveComponent(LogicComponentsLookup.ComGizmos);
            }
            else
            {
                _owner.ReplaceComponent(LogicComponentsLookup.ComGizmos, this);
            }
        }


        private void OnGizmos()
        {
            if (_owner == null || _gizmos.Count == 0)
                return;

            foreach (var action in new List<EntityGizmoAction>(_gizmos.Values))
            {
                action?.Invoke(_owner);
            }
        }
    }

    public partial class LogicEntity
    {
        public GizmosComponent comGizmos
        {
            get { return (GizmosComponent)GetComponent(LogicComponentsLookup.ComGizmos); }
        }

        public bool hasComGizmos
        {
            get { return HasComponent(LogicComponentsLookup.ComGizmos); }
        }

        public void AddGizmo(string name, EntityGizmoAction action)
        {
            if (string.IsNullOrEmpty(name) || action == null)
                return;

            var index = LogicComponentsLookup.ComGizmos;
            if (!hasComGizmos)
            {
                var component = (GizmosComponent)CreateComponent(index, typeof(GizmosComponent));
                AddComponent(index, component);
                component.AddGizmo(name, action);
            }
            else
            {
                comGizmos.AddGizmo(name, action);
            }
        }

        public void RemoveGizmo(string name)
        {
            if (!hasComGizmos)
                return;

            comGizmos.RemoveGizmo(name);
        }

        public void RemoveComGizmos()
        {
            if (hasComGizmos)
            {
                RemoveComponent(LogicComponentsLookup.ComGizmos);
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComGizmosIndex = new ComponentTypeIndex(typeof(GizmosComponent));
        public static int ComGizmos => _ComGizmosIndex.Index;
    }
}