namespace LccHotfix
{
    public class ComUniGameMode : MetaComponent
    {
        public GameModeBase mode;
    }

    public partial class MetaContext
    {
        public ComUniGameMode ComUniGameMode
        {
            get { return GetUniqueComponent<ComUniGameMode>(MetaComponentsLookup.ComUniGameMode); }
        }

        public bool hasComUniGameMode
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniGameMode); }
        }

        public void SetComUniGameMode(GameModeBase mode)
        {
            var index = MetaComponentsLookup.ComUniGameMode;
            var component = (ComUniGameMode)UniqueEntity.CreateComponent(index, typeof(ComUniGameMode));
            component.mode = mode;
            SetUniqueComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex ComUniGameModeIndex = new ComponentTypeIndex(typeof(ComUniGameMode));
        public static int ComUniGameMode => ComUniGameModeIndex.index;
    }
}