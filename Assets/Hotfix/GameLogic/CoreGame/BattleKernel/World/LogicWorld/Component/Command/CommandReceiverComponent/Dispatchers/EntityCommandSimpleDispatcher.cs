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
                foreach (var handler in OnHandleCommand.GetInvocationList())
                {
                    if (((ComponentHandleCommand)handler)(entity, cmd))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual void BindOwner(LogicEntity owner)
        {
            m_owner = owner;
            // 稀疏槽扫描，避免 GetComponents 冷缓存 ToArray 分配
            for (int i = 0, n = owner.totalComponents; i < n; i++)
            {
                if (!owner.HasComponent(i))
                    continue;

                var component = owner.GetComponent(i);
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
