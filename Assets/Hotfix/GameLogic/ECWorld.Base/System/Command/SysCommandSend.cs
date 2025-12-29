using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public sealed class SysCommandSend : ReactiveSystem<LogicEntity>
    {
        private LogicWorld _world;

        public SysCommandSend(LogicWorld world) : base(world)
        {
            _world = world;
        }

        protected override ICollector<LogicEntity> GetTrigger(IContext<LogicEntity> context)
        {
            return context.CreateCollector(LogicMatcher.AllOf(LogicComponentsLookup.ComCommandSender));
        }

        protected override bool Filter(LogicEntity entity)
        {
            return entity.hasComCommandSender;
        }

        protected override void Execute(List<LogicEntity> entities)
        {
            foreach (var e in entities)
            {
                e.comCommandSender.PreHandleCommand();
                e.comCommandSender.SendQueue.Clear();
            }
        }
    }
}