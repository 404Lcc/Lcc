using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public sealed class SysCommandReceive : ReactiveSystem<LogicEntity>
    {
        private LogicWorld _logicWorld;

        public SysCommandReceive(LogicWorld logicWorld) : base(logicWorld)
        {
            _logicWorld = logicWorld;
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