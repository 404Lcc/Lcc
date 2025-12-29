using UnityEngine;

namespace LccHotfix
{
    public interface ILocomotion : IDispose
    {
        Vector3 CurPosition { get; set; }
        Quaternion CurRotation { get; set; }
        Vector3 CurScale { get; set; }

        bool IsRuning { get; set; }

        void Update(float dt);
        bool IsEnd();

    }

    public class ComLocomotion : LogicComponent
    {
        private ILocomotion _locomotion;

        public ILocomotion Locomotion
        {
            get { return _locomotion; }
            set { _locomotion = value; }
        }

        public override void Dispose()
        {
            base.Dispose();

            _locomotion.Dispose();
        }
    }

    public partial class LogicEntity
    {
        public ComLocomotion comLocomotion
        {
            get { return (ComLocomotion)GetComponent(LogicComponentsLookup.ComLocomotion); }
        }

        public bool hasComLocomotion
        {
            get { return HasComponent(LogicComponentsLookup.ComLocomotion); }
        }


        public void ChangeLocomotion(ILocomotion locomotion)
        {
            if (!hasComTransform)
                return;

            locomotion.CurPosition = comTransform.position;
            locomotion.CurRotation = comTransform.rotation;
            locomotion.CurScale = comTransform.scale;

            if (hasComLocomotion)
            {
                var index = LogicComponentsLookup.ComLocomotion;
                var component = (ComLocomotion)CreateComponent(index, typeof(ComLocomotion));
                component.Locomotion = locomotion;
                ReplaceComponent(index, component);
            }
            else
            {
                var index = LogicComponentsLookup.ComLocomotion;
                var component = (ComLocomotion)CreateComponent(index, typeof(ComLocomotion));
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
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComLocomotionIndex = new ComponentTypeIndex(typeof(ComLocomotion));
        public static int ComLocomotion => ComLocomotionIndex.index;
    }
}