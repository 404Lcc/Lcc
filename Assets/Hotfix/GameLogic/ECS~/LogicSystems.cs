using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public interface ILateUpdateSystem : ISystem
    {
        void LateUpdate();
    }

    public class LogicSystems : Systems
    {
        private readonly List<ILateUpdateSystem> _lateUpdateSystemList = new List<ILateUpdateSystem>();

        public override Systems Add(ISystem system)
        {
            if (system is ILateUpdateSystem lateUpdateSystem)
            {
                _lateUpdateSystemList.Add(lateUpdateSystem);
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
    }
}