using Entitas;
using System;

namespace LccHotfix
{
    public partial class LogicWorld : Context<LogicEntity>
    {
        public LogicWorld(ContextInfo contextInfo, int totalComponents, Func<LogicEntity> entityFactory, int startCreationIndex = 0, Func<IEntity, IAERC> aercFactory = null) : base(totalComponents, startCreationIndex, contextInfo, aercFactory, entityFactory)
        {
            OnEntityCreated += EntityCreated;
            OnEntityDestroyed += EntityDestroyed;
            OnEntityWillBeDestroyed += EntityWillBeDestroyed;
        }

        private void EntityCreated(IContext context, IEntity entity)
        {
            ((LogicEntity)entity).EnterWorld(this);
        }

        private void EntityDestroyed(IContext context, IEntity entity)
        {
            ((LogicEntity)entity).LeaveWorld();
        }

        private void EntityWillBeDestroyed(IContext context, IEntity entity)
        {
            ((LogicEntity)entity).WillBeLeaveWorld();
        }

        public TIndex GetEntityIndex<TComponent, TIndex>()
        {
            return (TIndex)GetEntityIndex(typeof(TComponent).Name);
        }
    }
}