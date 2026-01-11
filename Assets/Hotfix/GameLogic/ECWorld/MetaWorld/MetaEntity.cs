using Entitas;

namespace LccHotfix
{
    public partial class MetaEntity : Entity
    {
        public MetaWorld OwnerWorld { get; private set; }

        private readonly EntityComponentChanged _addComponent;
        private readonly EntityComponentChanged _removeComponent;
        private readonly EntityComponentReplaced _replacedComponent;

        public MetaEntity()
        {
            _addComponent = OnAddComponent;
            _removeComponent = OnRemoveComponent;
            _replacedComponent = OnReplacedComponent;
        }

        public virtual void Enter(MetaWorld metaWorld)
        {
            OwnerWorld = metaWorld;
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

        public virtual void WillBeLeave()
        {
        }

        private void OnAddComponent(IEntity entity, int index, IComponent component)
        {
            if (component is MetaComponent)
            {
                ((MetaComponent)component).PostInitialize(this);
            }
        }

        private void OnRemoveComponent(IEntity entity, int index, IComponent component)
        {
            if (component is IComponentDispose dispose)
            {
                dispose.DisposeOnRemove();
            }
        }

        private void OnReplacedComponent(IEntity entity, int index, IComponent previousComponent, IComponent newComponent)
        {
            if (previousComponent == newComponent)
            {
                return;
            }

            if (previousComponent is IComponentDispose dispose)
            {
                dispose.DisposeOnRemove();
            }

            if (newComponent is MetaComponent)
            {
                ((MetaComponent)newComponent).PostInitialize(this);
            }
        }
    }
}