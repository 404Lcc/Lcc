namespace LccHotfix
{
    internal partial class Main : Module
    {
        public static Main Current { get; set; }
        
        public static ICodeTypesService CodeTypesService { get; set; }
        public static IGameObjectPoolService GameObjectPoolService { get; set; }
        public static IEasingService EasingService { get; set; }
        public static ICoroutineService CoroutineService { get; set; }
        public static IModelService ModelService { get; set; }
        public static ISaveService SaveService { get; set; }
        public static IFunctionOpenService FunctionOpenService { get; set; }
        public static IFXService FXService { get; set; }
        public static IEventService EventService { get; set; }
        public static ICameraService CameraService { get; set; }
        public static ITimerService TimerService { get; set; }
        public static IAssetService AssetService { get; set; }
        public static ISceneService SceneService { get; set; }
        
        public static void SetMain(Main main)
        {
            main.OnInstall();
        }
    }
}