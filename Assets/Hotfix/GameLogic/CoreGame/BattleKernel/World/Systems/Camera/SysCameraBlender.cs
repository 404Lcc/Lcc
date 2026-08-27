using Entitas;

namespace LccHotfix
{
    public class SysCameraBlender : ILateUpdateSystem
    {
        private readonly MetaWorld _metaWorld;

        public SysCameraBlender(ECWorlds world)
        {
            _metaWorld = world.MetaWorld;
        }

        public void LateUpdate()
        {
            TickCamera();
        }

        private void TickCamera()
        {
            if (!_metaWorld.hasComUniCameraBlender)
            {
                return;
            }

            ICameraBlender cameraBlender = _metaWorld.comUniCameraBlender.CameraBlender;
            cameraBlender.LateUpdate();
        }
    }
}
