using Entitas;
using RVO;

namespace LccHotfix
{
    public class SysORCA : IInitializeSystem, IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysORCA(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComORCA));
        }

        public void Initialize()
        {
            Simulator.Instance.setAgentDefaults(15f, 10, 10.0f, 10.0f, 3, 10, new RVO.Vector2(0.0f, 0.0f));
            Simulator.Instance.setTimeStep(0.02f);
        }

        public void Execute()
        {
            Simulator.Instance.doStep();

            foreach (var entity in _group.GetEntities())
            {
                var comORCA = entity.ComORCA;

                comORCA.Update();
            }
        }
    }
}