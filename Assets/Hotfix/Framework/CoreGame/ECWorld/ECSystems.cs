using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public interface ILateUpdateSystem : ISystem
    {
        void LateUpdate();
    }

    public interface IGizmosSystem : ISystem
    {
        void Gizmos();
    }

    public class ECSystems : Systems
    {
        private readonly List<ILateUpdateSystem> _lateUpdateSystemList = new List<ILateUpdateSystem>();
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
    }
}