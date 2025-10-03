using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysAStar : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysAStar(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComAStar, LogicComponentsLookup.ComTransform));
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comAStar = entity.comAStar;
                var comTransform = entity.comTransform;
                if (comAStar.isActive)
                {
                    comAStar.ctrl.SearchPath();
                    var newPos = comAStar.ctrl.position;
                    newPos.z = 0;
                    comTransform.SetPosition(newPos);
                    comTransform.SetRotation(comAStar.ctrl.rotation);
                }
                else
                {
                    comAStar.ctrl.SetPath(null);
                }
            }
        }
    }
}