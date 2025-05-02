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

        public MetaEntity comUniCameraBlenderEntity
        {
            get { return GetGroup(MetaMatcher.ComUniCameraBlender).GetSingleEntity(); }
        }

        public ComUniCameraBlender comUniCameraBlender
        {
            get { return comUniCameraBlenderEntity.comUniCameraBlender; }
        }

        public bool hasComUniCameraBlender
        {
            get { return comUniCameraBlenderEntity != null; }
        }

        public MetaEntity SetComUniCameraBlender(ICameraBlender newCameraBlender)
        {
            if (hasComUniCameraBlender)
            {
                var entity = comUniCameraBlenderEntity;
                entity.ReplaceComUniCameraBlender(newCameraBlender);
                return entity;
            }
            else
            {
                var entity = CreateEntity();
                entity.AddComUniCameraBlender(newCameraBlender);
                return entity;
            }
        }
    }

    public partial class MetaEntity
    {
        public ComUniCameraBlender comUniCameraBlender
        {
            get { return (ComUniCameraBlender)GetComponent(MetaComponentsLookup.ComUniCameraBlender); }
        }

        public bool hasComUniCameraBlender
        {
            get { return HasComponent(MetaComponentsLookup.ComUniCameraBlender); }
        }

        public void AddComUniCameraBlender(ICameraBlender newCameraBlender)
        {
            var index = MetaComponentsLookup.ComUniCameraBlender;
            var component = (ComUniCameraBlender)CreateComponent(index, typeof(ComUniCameraBlender));
            component.CameraBlender = newCameraBlender;
            AddComponent(index, component);
        }

        public void ReplaceComUniCameraBlender(ICameraBlender newCameraBlender)
        {
            var index = MetaComponentsLookup.ComUniCameraBlender;
            var component = (ComUniCameraBlender)CreateComponent(index, typeof(ComUniCameraBlender));
            component.CameraBlender = newCameraBlender;
            ReplaceComponent(index, component);
        }

        public void RemoveComUniCameraBlender()
        {
            var index = MetaComponentsLookup.ComUniCameraBlender;
            var component = (ComUniCameraBlender)CreateComponent(index, typeof(ComUniCameraBlender));
            RemoveComponent(MetaComponentsLookup.ComUniCameraBlender);
        }
    }

    public sealed partial class MetaMatcher
    {

        static Entitas.IMatcher<MetaEntity> _matcherComUniCameraBlender;

        public static Entitas.IMatcher<MetaEntity> ComUniCameraBlender
        {
            get
            {
                if (_matcherComUniCameraBlender == null)
                {
                    var matcher = (Entitas.Matcher<MetaEntity>)Entitas.Matcher<MetaEntity>.AllOf(MetaComponentsLookup.ComUniCameraBlender);
                    matcher.ComponentNames = MetaComponentsLookup.componentNames;
                    _matcherComUniCameraBlender = matcher;
                }

                return _matcherComUniCameraBlender;
            }
        }
    }

    public static partial class MetaComponentsLookup
    {
        public static int ComUniCameraBlender;
    }
}