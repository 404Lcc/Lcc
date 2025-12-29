using UnityEngine;
using Entitas;

namespace LccHotfix
{
    public class SysFSM : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysFSM(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComFSM));
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comFSM = entity.comFSM;
                comFSM.FSM.UpdateFSM();
            }
        }
    }
}