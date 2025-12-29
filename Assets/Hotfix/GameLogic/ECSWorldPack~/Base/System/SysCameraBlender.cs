using Entitas;

namespace LccHotfix
{
    public class SysCameraBlender : IExecuteSystem, ILateUpdateSystem
    {
        private MetaContext _metaContext;

        public SysCameraBlender(ECSWorld world)
        {
            _metaContext = world.MetaContext;
        }

        public void Execute()
        {
            if (_metaContext.hasComUniCameraBlender)
            {
                _metaContext.ComUniCameraBlender.CameraBlender.Update();
            }
        }

        public void LateUpdate()
        {
            if (_metaContext.hasComUniCameraBlender)
            {
                _metaContext.ComUniCameraBlender.CameraBlender.LateUpdate();
            }
        }
    }
}