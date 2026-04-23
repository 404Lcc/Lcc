using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public sealed class SysCommandSend : ReactiveSystem<LogicEntity>
    {
        private ECWorlds _world;
        //private INetworkService _netService;

        public SysCommandSend(ECWorlds world) : base(world.LogicWorld)
        {
            _world = world;
            //_netService = _world.Services.Network;
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
                //_netService.SendCommandsMessage(e.ID, e.CommandSender.SendQueue);
                e.comCommandSender.SendQueue.Clear();
            }
        }
    }
}