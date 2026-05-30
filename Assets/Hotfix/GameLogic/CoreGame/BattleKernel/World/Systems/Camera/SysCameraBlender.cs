using Entitas;

namespace LccHotfix
{
    public class SysCameraBlender : IExecuteSystem, ILateUpdateSystem
    {
        private MetaWorld _metaContext;

        public SysCameraBlender(ECWorlds world)
        {
            _metaContext = world.MetaWorld;
        }

        public void Execute()
        {
            if (_metaContext.hasComUniCameraBlender)
            {
                _metaContext.comUniCameraBlender.CameraBlender.Update();
            }
        }

        public void LateUpdate()
        {
            if (_metaContext.hasComUniCameraBlender)
            {
                _metaContext.comUniCameraBlender.CameraBlender.LateUpdate();
            }
        }
    }
}
