using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    // 驱动 CommandSenderComponent 的本地命令派发。当前项目未接入网络发送或 CommandReceiver 队列。
    public sealed class SysCommandSend : ReactiveSystem<LogicEntity>
    {
        public SysCommandSend(ECWorlds world) : base(world.LogicWorld)
        {
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
