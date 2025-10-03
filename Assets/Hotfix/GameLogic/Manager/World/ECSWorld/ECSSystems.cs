using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public interface ILateUpdateSystem : ISystem
    {
        void LateUpdate();
    }

    public interface IDrawGizmosSystem : ISystem
    {
        void DrawGizmos();
    }

    public class ECSSystems : Systems
    {
        private readonly List<ILateUpdateSystem> _lateUpdateSystemList = new List<ILateUpdateSystem>();
        private readonly List<IDrawGizmosSystem> _drawGizmosSystemList = new List<IDrawGizmosSystem>();

        public override Systems Add(ISystem system)
        {
            if (system is ILateUpdateSystem lateUpdateSystem)
            {
                _lateUpdateSystemList.Add(lateUpdateSystem);
            }

            if (system is IDrawGizmosSystem drawGizmosSystem)
            {
                _drawGizmosSystemList.Add(drawGizmosSystem);
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

        public void DrawGizmos()
        {
            foreach (var item in _drawGizmosSystemList)
            {
                item.DrawGizmos();
            }
        }
    }
}