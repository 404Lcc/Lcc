using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysControl : IExecuteSystem, ILateUpdateSystem
    {
        private IGroup<LogicEntity> _group;

        public SysControl(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComControl, LogicComponentsLookup.ComTransform));
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comControl = entity.comControl;
                comControl.GetControl<IControl>().Update();
            }
        }

        public void LateUpdate()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comControl = entity.comControl;
                comControl.GetControl<IControl>().LateUpdate();
            }
        }
    }
}