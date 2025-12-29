using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public sealed class SysCommandSend : ReactiveSystem<LogicEntity>
    {
        private LogicWorld _logicWorld;

        public SysCommandSend(LogicWorld logicWorld) : base(logicWorld)
        {
            _logicWorld = logicWorld;
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