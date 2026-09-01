using Entitas;

namespace LccHotfix
{
    public class SysCameraBlender : ILateUpdateSystem
    {
        private readonly MetaWorld _metaWorld;
        private readonly LogicWorld _logicWorld;

        public SysCameraBlender(ECWorlds world)
        {
            _metaWorld = world.MetaWorld;
            _logicWorld = world.LogicWorld;
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
            cameraBlender.BindLogicWorld(_logicWorld);
            cameraBlender.LateUpdate();
        }
    }
}
