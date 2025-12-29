using Entitas;

namespace LccHotfix
{
    public partial class LogicEntity : Entity
    {
        public LogicWorld OwnerWorld { get; private set; }

        private readonly EntityComponentChanged _addComponent;
        private readonly EntityComponentChanged _removeComponent;
        private readonly EntityComponentReplaced _replacedComponent;

        public LogicEntity()
        {
            _addComponent = OnAddComponent;
            _removeComponent = OnRemoveComponent;
            _replacedComponent = OnReplacedComponent;
        }

        public virtual void EnterWorld(LogicWorld logicWorld)
        {
            OwnerWorld = logicWorld;
            OnComponentAdded += _addComponent;
            OnComponentRemoved += _removeComponent;
            OnComponentReplaced += _replacedComponent;
        }

        public virtual void WillBeLeaveWorld()
        {
        }

        public virtual void LeaveWorld()
        {
            OnComponentAdded -= _addComponent;
            OnComponentRemoved -= _removeComponent;
            OnComponentReplaced -= _replacedComponent;
        }

        protected void OnAddComponent(Entity entity, int index, IComponent component)
        {
            if (component is LogicComponent)
            {
                ((LogicComponent)component).PostInitialize(this);
            }
        }

        protected void OnRemoveComponent(Entity entity, int index, IComponent component)
        {
            if (component is IComponentDispose dispose)
            {
                dispose.DisposeOnRemove();
            }
        }

        protected void OnReplacedComponent(Entity entity, int index, IComponent previousComponent, IComponent newComponent)
        {
            if (previousComponent == newComponent)
            {
                return;
            }

            if (previousComponent is IComponentDispose dispose)
            {
                dispose.DisposeOnRemove();
            }

            if (newComponent is LogicComponent)
            {
                ((LogicComponent)newComponent).PostInitialize(this);
            }
        }
    }
}