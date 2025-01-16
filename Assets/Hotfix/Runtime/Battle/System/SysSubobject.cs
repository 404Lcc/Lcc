using Entitas;

namespace LccHotfix
{
    public class SysSubobject : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysSubobject(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComSubobject, LogicMatcher.ComTransform));
        }


        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comSubobject = entity.comSubobject;
                if (comSubobject.Agent != null)
                {
                    comSubobject.Agent.Update();
                }
            }
        }
    }
}