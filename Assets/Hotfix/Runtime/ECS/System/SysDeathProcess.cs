using Entitas;

namespace LccHotfix
{
    public class SysDeathProcess : IExecuteSystem
    {
        private ECSWorld _world;
        private IGroup<LogicEntity> _group;

        public SysDeathProcess(ECSWorld world)
        {
            _world = world;
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComDeath));
        }

        void IExecuteSystem.Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comDeath = entity.comDeath;
                if (comDeath == null)
                    continue;
                var process = comDeath.deathProcess;
                if (process == null)
                {
                    DoDestroy(entity);
                }
                else
                {
                    process.Update();

                    if (process.IsFinished())
                    {
                        DoDestroy(entity);
                    }
                }
            }
        }

        private void DoDestroy(LogicEntity entity)
        {
            //todo 有一些组件要提前删除

            if (entity.IsEnabled)
            {
                entity.Destroy();
            }
        }
    }
}