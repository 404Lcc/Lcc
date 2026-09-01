using Entitas;
using System;

namespace LccHotfix
{
    public partial class LogicWorld : Context<LogicEntity>
    {
        private readonly IWorldCreationInfo _creationInfo;

        public LogicWorld(ContextInfo contextInfo, int totalComponents, Func<LogicEntity> entityFactory, int startCreationIndex = 0, Func<IEntity, IAERC> aercFactory = null, IWorldCreationInfo creationInfo = null) : base(totalComponents, startCreationIndex, contextInfo, aercFactory, entityFactory)
        {
            _creationInfo = creationInfo;
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

        public T GetCreationInfo<T>() where T : IWorldCreationInfo
        {
            return (T)_creationInfo;
        }
    }
}
