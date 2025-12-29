namespace LccHotfix
{
    public class ComUniCameraBlender : MetaComponent
    {
        public ICameraBlender CameraBlender;

        public override void PostInitialize(MetaEntity owner)
        {
            base.PostInitialize(owner);

            CameraBlender.PostInitialize();
        }

        public override void Dispose()
        {
            base.Dispose();

            CameraBlender.Dispose();
        }
    }

    public partial class MetaContext
    {
        public ComUniCameraBlender ComUniCameraBlender
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
        public static int ComUniCameraBlender => ComUniCameraBlenderIndex.index;
    }
}