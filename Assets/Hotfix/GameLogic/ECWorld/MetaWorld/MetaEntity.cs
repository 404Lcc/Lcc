using System;
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
            if (component is MetaComponent theCmpt)
            {
                SafePostInitialize(theCmpt);
            }
        }

        private void OnRemoveComponent(IEntity entity, int index, IComponent component)
        {
            if (component is IComponentDispose dispose)
            {
                SafeDisposeOnRemove(dispose);
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
                SafeDisposeOnRemove(dispose);
            }

            if (newComponent is MetaComponent theCmpt)
            {
                SafePostInitialize(theCmpt);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        protected void SafePostInitialize(MetaComponent theCmpt)
        {
            try
            {
                theCmpt.PostInitialize(this);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"MetaComponent PostInitialize catch Exception:{e}");
            }
        }
        protected void SafeDisposeOnRemove(IComponentDispose dispose)
        {
            try
            {
                dispose.DisposeOnRemove();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"MetaComponent DisposeOnRemove catch Exception:{e}");
            }
        }
    }
}