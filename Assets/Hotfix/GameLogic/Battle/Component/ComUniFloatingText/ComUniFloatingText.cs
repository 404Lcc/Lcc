namespace LccHotfix
{
    public class ComUniFloatingText : MetaComponent
    {
        public IFloatingText FloatingText;

        public override void PostInitialize(MetaEntity owner)
        {
            base.PostInitialize(owner);

            FloatingText.PostInitialize();
        }

        public override void Dispose()
        {
            base.Dispose();

            FloatingText.Dispose();
        }
    }

    public partial class MetaContext
    {

        public MetaEntity ComUniFloatingTextEntity
        {
            get { return GetGroup(MetaMatcher.ComUniFloatingText).GetSingleEntity(); }
        }

        public ComUniFloatingText ComUniFloatingText
        {
            get { return ComUniFloatingTextEntity.ComUniFloatingText; }
        }

        public bool hasComUniFloatingText
        {
            get { return ComUniFloatingTextEntity != null; }
        }

        public MetaEntity SetComUniFloatingText(IFloatingText newFloatingText)
        {
            if (hasComUniFloatingText)
            {
                var entity = ComUniFloatingTextEntity;
                entity.ReplaceComUniFloatingText(newFloatingText);
                return entity;
            }
            else
            {
                var entity = CreateEntity();
                entity.AddComUniFloatingText(newFloatingText);
                return entity;
            }
        }
    }

    public partial class MetaEntity
    {
        public ComUniFloatingText ComUniFloatingText
        {
            get { return (ComUniFloatingText)GetComponent(MetaComponentsLookup.ComUniFloatingText); }
        }

        public bool hasComUniFloatingText
        {
            get { return HasComponent(MetaComponentsLookup.ComUniFloatingText); }
        }

        public void AddComUniFloatingText(IFloatingText newFloatingText)
        {
            var index = MetaComponentsLookup.ComUniFloatingText;
            var component = (ComUniFloatingText)CreateComponent(index, typeof(ComUniFloatingText));
            component.FloatingText = newFloatingText;
            AddComponent(index, component);
        }

        public void ReplaceComUniFloatingText(IFloatingText newFloatingText)
        {
            var index = MetaComponentsLookup.ComUniFloatingText;
            var component = (ComUniFloatingText)CreateComponent(index, typeof(ComUniFloatingText));
            component.FloatingText = newFloatingText;
            ReplaceComponent(index, component);
        }

        public void RemoveComUniFloatingText()
        {
            var index = MetaComponentsLookup.ComUniFloatingText;
            var component = (ComUniFloatingText)CreateComponent(index, typeof(ComUniFloatingText));
            RemoveComponent(MetaComponentsLookup.ComUniFloatingText);
        }
    }

    public sealed partial class MetaMatcher
    {

        static Entitas.IMatcher<MetaEntity> _matcherComUniFloatingText;

        public static Entitas.IMatcher<MetaEntity> ComUniFloatingText
        {
            get
            {
                if (_matcherComUniFloatingText == null)
                {
                    var matcher = (Entitas.Matcher<MetaEntity>)Entitas.Matcher<MetaEntity>.AllOf(MetaComponentsLookup.ComUniFloatingText);
                    matcher.ComponentNames = MetaComponentsLookup.componentNames;
                    _matcherComUniFloatingText = matcher;
                }

                return _matcherComUniFloatingText;
            }
        }
    }

    public static partial class MetaComponentsLookup
    {
        public static int ComUniFloatingText;
    }
}