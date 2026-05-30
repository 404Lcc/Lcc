using HotUpdate.Framework;
using UnityEngine;

namespace LccHotfix
{
    public interface ILocomotion : IReference
    {
        Vector3 DeltaPosition { get; }
        Quaternion DeltaRotation { get; }
        void BeforeUpdate();
        void Update(float dt, LogicEntity entity);
        void LateUpdate(float dt, LogicEntity entity);
        bool IsEnd();
    }

    public interface ILocomotionSpeed
    {
        public void SetMoveSpeed(float speed);
    }

    public class LocomotionComponent : LogicComponent
    {
        private ILocomotion _locomotion;

        public ILocomotion Locomotion
        {
            get { return _locomotion; }
            set { _locomotion = value; }
        }


        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();

            if (Locomotion != null)
            {
                ReferencePool.Release(Locomotion);
                Locomotion = null;
            }
        }
    }


    public partial class LogicEntity
    {
        public LocomotionComponent comLocomotion
        {
            get { return (LocomotionComponent)GetComponent(LogicComponentsLookup.ComLocomotion); }
        }

        public bool hasComLocomotion
        {
            get { return HasComponent(LogicComponentsLookup.ComLocomotion); }
        }

        private void ReplaceComLocomotion(ILocomotion locomotion)
        {
            if (!hasComTransform)
                return;

            // locomotion.CurPosition = comTransform.position;
            // locomotion.CurRotation = comTransform.rotation;
            // locomotion.CurScale = comTransform.scale;

            if (hasComLocomotion)
            {
                var index = LogicComponentsLookup.ComLocomotion;
                var component = (LocomotionComponent)CreateComponent(index, typeof(LocomotionComponent));
                if (component.Locomotion != null)
                {
                    ReferencePool.Release(component.Locomotion);
                    component.Locomotion = null;
                }
                component.Locomotion = locomotion;
                ReplaceComponent(index, component);
            }
            else
            {
                var index = LogicComponentsLookup.ComLocomotion;
                var component = (LocomotionComponent)CreateComponent(index, typeof(LocomotionComponent));
                component.Locomotion = locomotion;
                AddComponent(index, component);
            }
        }
        
        public void RemoveComLocomotion()
        {
            if (hasComLocomotion)
            {
                RemoveComponent(LogicComponentsLookup.ComLocomotion);
            }
        }
        
        public T GetLocomotion<T>() where T : class
        {
            if (hasComLocomotion)
            {
                return comLocomotion.Locomotion as T;
            }

            return null;
        }
        
        public void SetComLocomotion<T>(out T locomotion) where T : class, ILocomotion, IReference, new()
        {
            locomotion = ReferencePool.Acquire<T>();
            ReplaceComLocomotion(locomotion);
        }
        
        public void SetComLocomotion(ILocomotion locomotion)
        {
            ReplaceComLocomotion(locomotion);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComLocomotionIndex = new(typeof(LocomotionComponent));
        public static int ComLocomotion => _ComLocomotionIndex.Index;
    }
}
