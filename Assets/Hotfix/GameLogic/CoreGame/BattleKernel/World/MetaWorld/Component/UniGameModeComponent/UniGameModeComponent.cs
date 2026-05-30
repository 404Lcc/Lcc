namespace LccHotfix
{
    public class UniGameModeComponent : MetaComponent
    {
        public BattleModeLogic GameModeLogic { get; private set; }
        public DamageRecorder DmgRecorder  { get; private set; } 
        private IBattleModeLogicService _modeLogicService;

        public void Init(ICustomLogicGenInfo genInfo, IBattleModeLogicService modeLogicService)
        {
            _modeLogicService = modeLogicService;
            GameModeLogic = _modeLogicService.CreateModeLogic(genInfo);
            DmgRecorder = new DamageRecorder();
        }

        public override void DisposeOnRemove()
        {
            if (GameModeLogic != null)
            {
                _modeLogicService?.DestroyModeLogic(GameModeLogic);
                GameModeLogic = null;
            }

            _modeLogicService = null;

            base.DisposeOnRemove();
        }
    }

    public partial class MetaWorld
    {
        public UniGameModeComponent comUniGameMode
        {
            get { return GetUniqueComponent<UniGameModeComponent>(MetaComponentsLookup.ComUniGameMode); }
        }

        public bool hasComUniGameMode
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniGameMode); }
        }
        
        public void SetComUniGameMode(ICustomLogicGenInfo genInfo, IBattleModeLogicService modeLogicService)
        {
            var index = MetaComponentsLookup.ComUniGameMode;
            var component = (UniGameModeComponent)UniqueEntity.CreateComponent(index, typeof(UniGameModeComponent));
            component.Init(genInfo, modeLogicService);
            SetUniqueComponent(index, component);
        }
        
        public void RemoveComUniGameMode()
        {
            if (hasComUniGameMode)
            {
                UniqueEntity.RemoveComponent(MetaComponentsLookup.ComUniGameMode);    
            }
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex _ComUniGameModeIndex = new(typeof(UniGameModeComponent));
        public static int ComUniGameMode => _ComUniGameModeIndex.Index;
    }
}
