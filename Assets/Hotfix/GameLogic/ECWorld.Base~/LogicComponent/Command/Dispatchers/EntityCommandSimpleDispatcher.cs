using Entitas;

namespace LccHotfix
{
    public delegate bool ComponentHandleCommand(LogicEntity entity, EntityCommand cmd);

    public class EntityCommandSimpleDispatcher : IEntityCommandDispatcher
    {
        protected LogicEntity _owner;
        protected event ComponentHandleCommand _onHandleCommand;

        public virtual bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            if (_onHandleCommand != null)
            {
                _onHandleCommand(entity, cmd);
            }

            return true;
        }

        public virtual void BindOwner(LogicEntity owner)
        {
            _owner = owner;
            foreach (var component in owner.GetComponents())
            {
                if (component is IEntityCommandHandler commandHandler)
                {
                    _onHandleCommand += commandHandler.HandleEntityCommand;
                }
            }

            owner.OnComponentAdded += OnComponentAdded;
            owner.OnComponentRemoved += OnComponentRemoved;
        }

        public virtual void UnBindOwner()
        {
            _owner.OnComponentAdded -= OnComponentAdded;
            _owner.OnComponentRemoved -= OnComponentRemoved;
            _owner = null;
            _onHandleCommand = null;
        }

        private void OnComponentAdded(Entity entity, int index, IComponent component)
        {
            if (component is IEntityCommandHandler commandHandler)
            {
                _onHandleCommand += commandHandler.HandleEntityCommand;
            }
        }

        private void OnComponentRemoved(Entity entity, int index, IComponent component)
        {
            if (component is IEntityCommandHandler commandHandler)
            {
                _onHandleCommand -= commandHandler.HandleEntityCommand;
            }
        }
    }
}