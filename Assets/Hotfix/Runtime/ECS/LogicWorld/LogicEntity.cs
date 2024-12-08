using Entitas;

namespace LccHotfix
{
    public partial class LogicEntity : Entity
    {
        public LogicContext OwnerWorld { get; private set; }

        private readonly EntityComponentChanged _addComponent;
        private readonly EntityComponentChanged _removeComponent;
        private readonly EntityComponentReplaced _replacedComponent;

        public LogicEntity()
        {
            _addComponent = OnAddComponent;
            _removeComponent = OnRemoveComponent;
            _replacedComponent = OnReplacedComponent;
        }

        public virtual void Enter(LogicContext logicContext)
        {
            OwnerWorld = logicContext;
            OnComponentAdded += _addComponent;
            OnComponentRemoved += _removeComponent;
            OnComponentReplaced += _replacedComponent;
        }

        public virtual void Leave()
        {
            OnComponentAdded -= _addComponent;
            OnComponentRemoved -= _removeComponent;
            OnComponentReplaced -= _replacedComponent;
        }

        private void OnAddComponent(Entity entity, int index, IComponent component)
        {
            if (component is LogicComponent)
            {
                ((LogicComponent)component).PostInitialize(this);
            }
        }

        private void OnRemoveComponent(Entity entity, int index, IComponent component)
        {
            if (component is IDispose dispose)
            {
                dispose.Dispose();
            }
        }

        private void OnReplacedComponent(Entity entity, int index, IComponent previousComponent, IComponent newComponent)
        {
            if (previousComponent is IDispose dispose)
            {
                dispose.Dispose();
            }

            if (newComponent is LogicComponent)
            {
                ((LogicComponent)newComponent).PostInitialize(this);
            }
        }
    }
}