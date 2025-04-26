using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysControl : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysControl(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComControl, LogicMatcher.ComProperty, LogicMatcher.ComTransform));
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comProp = entity.comProperty;
                var comControl = entity.comControl;
                comControl.control.Update();
            }
        }
    }
}