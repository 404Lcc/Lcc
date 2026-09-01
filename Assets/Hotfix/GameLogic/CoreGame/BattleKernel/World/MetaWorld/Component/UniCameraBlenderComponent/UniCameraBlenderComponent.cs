using UnityEngine;

namespace LccHotfix
{
    public interface ICameraBlender
    {
        public bool IsActive { get; set; }
        public Transform Target { get; set; }
        void PostInitialize();
        void Dispose();
        void Update();
        void LateUpdate();
        void ChangeTarget(Transform target);
        void ShakeCamera(float intensity = 0.5f, float duration = 0.5f);

        /// <summary>
        /// 每帧 LateUpdate 前绑定 LogicWorld（索敌/小队取景等）。不需要的实现可空实现。
        /// </summary>
        void BindLogicWorld(LogicWorld logicWorld);
    }
    
    public class ComUniCameraBlender : MetaComponent
    {
        public ICameraBlender CameraBlender;

        public override void PostInitialize(MetaEntity owner)
        {
            base.PostInitialize(owner);

            CameraBlender.PostInitialize();
        }

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();

            CameraBlender.Dispose();
        }
    }

    public partial class MetaWorld
    {
        public ComUniCameraBlender comUniCameraBlender
        {
            get { return GetUniqueComponent<ComUniCameraBlender>(MetaComponentsLookup.ComUniCameraBlender); }
        }

        public bool hasComUniCameraBlender
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniCameraBlender); }
        }

        public void SetComUniCameraBlender(ICameraBlender cameraBlender)
        {
            var index = MetaComponentsLookup.ComUniCameraBlender;
            var component = (ComUniCameraBlender)UniqueEntity.CreateComponent(index, typeof(ComUniCameraBlender));
            component.CameraBlender = cameraBlender;
            SetUniqueComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex ComUniCameraBlenderIndex = new ComponentTypeIndex(typeof(ComUniCameraBlender));
        public static int ComUniCameraBlender => ComUniCameraBlenderIndex.Index;
    }
}
