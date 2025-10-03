namespace LccHotfix
{
    public class ComUniGameMode : MetaComponent
    {
        public GameModeBase mode;
    }
    public partial class MetaContext
    {
        public MetaEntity comUniGameModeEntity { get { return GetGroup(MetaMatcher.ComUniGameMode).GetSingleEntity(); } }
        public ComUniGameMode comUniGameMode { get { return comUniGameModeEntity.comUniGameMode; } }
        public bool hasComUniGameMode { get { return comUniGameModeEntity != null; } }

        public MetaEntity SetComUniGameMode(GameModeBase newMode)
        {
            if (hasComUniGameMode)
            {
                var entity = comUniGameModeEntity;
                entity.ReplaceComUniGameMode(newMode);
                return entity;
            }
            else
            {
                var entity = CreateEntity();
                entity.AddComUniGameMode(newMode);
                return entity;
            }
        }
    }
    public partial class MetaEntity
    {

        public ComUniGameMode comUniGameMode { get { return (ComUniGameMode)GetComponent(MetaComponentsLookup.ComUniGameMode); } }
        public bool hasComUniGameMode { get { return HasComponent(MetaComponentsLookup.ComUniGameMode); } }

        public void AddComUniGameMode(GameModeBase newMode)
        {
            var index = MetaComponentsLookup.ComUniGameMode;
            var component = (ComUniGameMode)CreateComponent(index, typeof(ComUniGameMode));
            component.mode = newMode;
            AddComponent(index, component);
        }

        public void ReplaceComUniGameMode(GameModeBase newMode)
        {
            var index = MetaComponentsLookup.ComUniGameMode;
            var component = (ComUniGameMode)CreateComponent(index, typeof(ComUniGameMode));
            component.mode = newMode;
            ReplaceComponent(index, component);
        }

        public void RemoveComUniGameMode()
        {
            RemoveComponent(MetaComponentsLookup.ComUniGameMode);
        }
    }
    public sealed partial class MetaMatcher
    {

        private static Entitas.IMatcher<MetaEntity> _matcherComUniGameMode;

        public static Entitas.IMatcher<MetaEntity> ComUniGameMode
        {
            get
            {
                if (_matcherComUniGameMode == null)
                {
                    var matcher = (Entitas.Matcher<MetaEntity>)Entitas.Matcher<MetaEntity>.AllOf(MetaComponentsLookup.ComUniGameMode);
                    matcher.ComponentNames = MetaComponentsLookup.componentNames;
                    _matcherComUniGameMode = matcher;
                }

                return _matcherComUniGameMode;
            }
        }
    }
    public static partial class MetaComponentsLookup
    {
        public static int ComUniGameMode;
    }
}