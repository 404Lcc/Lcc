using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    // 当前未接入系统流水线；保留给未来需要 CommandReceiver 队列时启用。
    public sealed class SysCommandReceive : ReactiveSystem<LogicEntity>
    {
        public SysCommandReceive(ECWorlds world) : base(world.LogicWorld)
        {
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
