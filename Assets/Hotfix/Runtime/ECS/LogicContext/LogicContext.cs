using Entitas;
using System;

namespace LccHotfix
{
    public class LogicContext : Context<LogicEntity>
    {
        private readonly static int _totalComponents = LogicComponentsLookup.TotalComponents;
        private readonly static int _startCreationIndex = 0;
        private readonly static ContextInfo _contextInfo = new ContextInfo("Logic", LogicComponentsLookup.componentNames.ToArray(), LogicComponentsLookup.componentTypes.ToArray());
        private readonly static Func<Entity, IAERC> _aercFactory = (entity) => new SafeAERC(entity);
        private readonly static Func<LogicEntity> _entityFactory = () => new LogicEntity();

        public LogicContext() : base(_totalComponents, _startCreationIndex, _contextInfo, _aercFactory, _entityFactory)
        {
            OnEntityCreated += EntityCreated;
            OnEntityDestroyed += EntityDestroyed;
        }
        private void EntityCreated(IContext context, Entity entity)
        {
            ((LogicEntity)entity).Enter(this);
        }
        private void EntityDestroyed(IContext context, Entity entity)
        {
            ((LogicEntity)entity).Leave();
        }
    }
}