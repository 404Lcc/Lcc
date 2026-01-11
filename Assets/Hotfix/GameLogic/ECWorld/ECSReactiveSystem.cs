using System.Collections.Generic;
using Entitas;

namespace LccHotfix
{
    public abstract class ECSReactiveSystem : ReactiveSystem<LogicEntity>
    {
        protected ECWorlds _world;

        public ECSReactiveSystem(ECWorlds world) : base(world.LogicWorld)
        {
            _world = world;
        }

        protected override ICollector<LogicEntity> GetTrigger(IContext<LogicEntity> context)
        {
            return null;
        }

        protected override bool Filter(LogicEntity entity)
        {
            return false;
        }

        protected override void Execute(List<LogicEntity> entities)
        {
        }
    }
}