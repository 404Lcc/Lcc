using System;
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

        protected void OnAddComponent(IEntity entity, int index, IComponent component)
        {
            if (component is LogicComponent theCmpt)
            {
                SafePostInitialize(theCmpt);
            }
        }

        protected void OnRemoveComponent(IEntity entity, int index, IComponent component)
        {
            if (component is IComponentDispose dispose)
            {
                SafeDisposeOnRemove(dispose);
            }
        }
        
        protected void OnReplacedComponent(IEntity entity, int index, IComponent previousComponent, IComponent newComponent)
        {
            if (previousComponent == newComponent)
            {
                return;
            }
            if (previousComponent is IComponentDispose dispose)
            {
                SafeDisposeOnRemove(dispose);
            }
            if (newComponent is LogicComponent theCmpt)
            {
                SafePostInitialize(theCmpt);
            }
        }
        
        //////////////////////////////////////////////////////////////////////////
        protected void SafePostInitialize(LogicComponent theCmpt)
        {
            try
            {
                theCmpt.PostInitialize(this);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"LogicComponent PostInitialize catch Exception:{e}");
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
                UnityEngine.Debug.LogError($"LogicComponent DisposeOnRemove catch Exception:{e}");
            }
        }

    }
}