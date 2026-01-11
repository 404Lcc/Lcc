using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public sealed class SysCommandSend : ReactiveSystem<LogicEntity>
    {
        private LogicWorld m_world;
        //private INetworkService m_netService;

        public SysCommandSend(LogicWorld world)
            : base(world)
        {
            m_world = world;
            //m_netService = m_world.Services.Network;
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
                //m_netService.SendCommandsMessage(e.ID, e.CommandSender.SendQueue);
                e.comCommandSender.SendQueue.Clear();
            }
        }

    }
}