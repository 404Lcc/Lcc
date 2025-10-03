using Entitas;
using System;

namespace LccHotfix
{
    public class LogicContext : Context<LogicEntity>
    {
        public LogicContext(int totalComponents, int startCreationIndex, ContextInfo contextInfo, Func<Entity, IAERC> aercFactory, Func<LogicEntity> entityFactory) : base(totalComponents, startCreationIndex, contextInfo, aercFactory, entityFactory)
        {
            OnEntityCreated += EntityCreated;
            OnEntityDestroyed += EntityDestroyed;
            OnEntityWillBeDestroyed += EntityWillBeDestroyed;
        }

        private void EntityCreated(IContext context, Entity entity)
        {
            ((LogicEntity)entity).Enter(this);
        }

        private void EntityDestroyed(IContext context, Entity entity)
        {
            ((LogicEntity)entity).Leave();
        }

        private void EntityWillBeDestroyed(IContext context, Entity entity)
        {
            ((LogicEntity)entity).WillBeLeave();
        }
    }
}