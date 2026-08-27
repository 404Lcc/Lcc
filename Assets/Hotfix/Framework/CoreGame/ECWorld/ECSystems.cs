using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public interface ILateUpdateSystem : ISystem
    {
        void LateUpdate();
    }

    public interface IFixedUpdateSystem : ISystem
    {
        void FixedUpdate(float dt, float dt_unscaled);
    }

    public interface IGizmosSystem : ISystem
    {
        void Gizmos();
    }

    public class ECSystems : Systems
    {
        private readonly List<ILateUpdateSystem> _lateUpdateSystemList = new List<ILateUpdateSystem>();
        private readonly List<IFixedUpdateSystem> _fixedUpdateSystemList = new List<IFixedUpdateSystem>();
        private readonly List<IGizmosSystem> _gizmosSystemList = new List<IGizmosSystem>();

        public override Systems Add(ISystem system)
        {
            if (system is ILateUpdateSystem lateUpdateSystem)
            {
                _lateUpdateSystemList.Add(lateUpdateSystem);
            }

            if (system is IGizmosSystem drawGizmosSystem)
            {
                _gizmosSystemList.Add(drawGizmosSystem);
            }

            if (system is IFixedUpdateSystem fixedUpdateSystem)
            {
                _fixedUpdateSystemList.Add(fixedUpdateSystem);
            }

            return base.Add(system);
        }

        public void LateUpdate()
        {
            foreach (var item in _lateUpdateSystemList)
            {
                item.LateUpdate();
            }
        }

        public void Gizmos()
        {
            foreach (var item in _gizmosSystemList)
            {
                item.Gizmos();
            }
        }

        public void FixedUpdate(float dt, float dt_unscaled)
        {
            foreach (var item in _fixedUpdateSystemList)
            {
                item.FixedUpdate(dt, dt_unscaled);
            }
        }
    }
}
