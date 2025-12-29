using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public sealed class SysCommandReceive : ReactiveSystem<LogicEntity>
    {
        private LogicWorld _world;

        public SysCommandReceive(LogicWorld world) : base(world)
        {
            _world = world;
        }

        protected override ICollector<LogicEntity> GetTrigger(IContext<LogicEntity> context)
        {
            return context.CreateCollector(LogicMatcher.AllOf(LogicComponentsLookup.ComCommandReceiver));
        }

        protected override bool Filter(LogicEntity entity)
        {
            return entity.hasComCommandReceiver;
        }

        protected override void Execute(List<LogicEntity> entities)
        {
            foreach (var e in entities)
            {
                e.comCommandReceiver.Dispatch();
            }
        }
    }
}