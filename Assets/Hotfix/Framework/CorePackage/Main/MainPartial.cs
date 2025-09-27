namespace LccHotfix
{
    internal partial class Main : Module
    {
        public static Main Current { get; set; }
        
        public static ICodeTypesService CodeTypesService { get; set; }
        public static IAssetService AssetService { get; set; }
        public static IGameObjectPoolService GameObjectPoolService { get; set; }
        public static ICoroutineService CoroutineService { get; set; }
        public static INetworkService NetworkService { get; set; }
        public static IModelService ModelService { get; set; }
        public static ISaveService SaveService { get; set; }
        public static ISettingService SettingService { get; set; }
        public static IAudioService AudioService { get; set; }
        public static IVibrationService VibrationService { get; set; }
        public static IFXService FXService { get; set; }
        public static IEventService EventService { get; set; }
        public static ICameraService CameraService { get; set; }
        public static ITimerService TimerService { get; set; }
        public static IProcedureService ProcedureService { get; set; }
        public static IWindowService WindowService { get; set; }
        public static IThreadSyncService ThreadSyncService { get; set; }
        public static IGizmoService GizmoService { get; set; }
        
        public static void SetMain(Main main)
        {
            main.OnInstall();
        }
    }
}