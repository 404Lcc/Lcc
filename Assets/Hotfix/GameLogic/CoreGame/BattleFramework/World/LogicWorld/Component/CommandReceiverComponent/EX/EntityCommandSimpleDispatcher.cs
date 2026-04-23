using Entitas;

namespace LccHotfix
{
    public delegate bool ComponentHandleCommand(LogicEntity entity, EntityCommand cmd);

    public class EntityCommandSimpleDispatcher : IEntityCommandDispatcher
    {
        protected event ComponentHandleCommand OnHandleCommand;
        protected LogicEntity m_owner;

        public virtual bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            if (OnHandleCommand != null)
            {
                OnHandleCommand(entity, cmd);
            }

            return true;
        }

        public virtual void BindOwner(LogicEntity owner)
        {
            m_owner = owner;
            foreach (var component in owner.GetComponents())
            {
                if (component is IEntityCommandHandler commandHandler)
                {
                    OnHandleCommand += commandHandler.HandleEntityCommand;
                }
            }

            owner.OnComponentAdded += _onComponentAdded;
            owner.OnComponentRemoved += _onComponentRemoved;
        }

        public virtual void UnBindOwner()
        {
            m_owner.OnComponentAdded -= _onComponentAdded;
            m_owner.OnComponentRemoved -= _onComponentRemoved;
            m_owner = null;
            OnHandleCommand = null;
        }

        private void _onComponentAdded(IEntity entity, int index, IComponent component)
        {
            if (component is IEntityCommandHandler commandHandler)
            {
                OnHandleCommand += commandHandler.HandleEntityCommand;
            }
        }

        private void _onComponentRemoved(IEntity entity, int index, IComponent component)
        {
            if (component is IEntityCommandHandler commandHandler)
            {
                OnHandleCommand -= commandHandler.HandleEntityCommand;
            }
        }
    }
}