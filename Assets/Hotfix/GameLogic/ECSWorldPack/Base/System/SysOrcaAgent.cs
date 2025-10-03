using Entitas;
using LccHotfix;

namespace LccHotfix
{
    public class SysOrcaAgent : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysOrcaAgent(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComOrcaAgent));
        }

        public void Execute()
        {
            foreach (var item in _group.GetEntities())
            {
                item.ComOrcaAgent.Update();
            }
        }
    }
}