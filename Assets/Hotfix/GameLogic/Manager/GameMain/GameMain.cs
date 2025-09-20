using System.Reflection;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    internal class GameMain : FrameworkMain
    {
        public override void OnInstall()
        {
            if (Current != null)
            {
                return;
            }

            base.OnInstall();

            CodeTypesService = Current.AddModule<CodeTypesManager>();
            CodeTypesService.LoadTypes(new Assembly[] { Launcher.Instance.HotfixAssembly });
            AssetService = Current.AddModule<AssetManager>();
            AssetService.SetHelper(new DefaultAssetHelper());
            GameObjectPoolService = Current.AddModule<GameObjectPoolManager>();
            GameObjectPoolService.SetLoader((location, root) =>
            {
                Main.AssetService.LoadRes<GameObject>(root, location, out var res);
                return res;
            });
            EasingService = Current.AddModule<EasingManager>();
            CoroutineService = Current.AddModule<CoroutineManager>();
            CoroutineService.SetCoroutineHelper(new DefaultCoroutineHelper());
            ModelService = Current.AddModule<ModelManager>();
            SaveService = Current.AddModule<SaveManager>();
            SaveService.SetSaveHelper(new DefaultSaveHelper());
            AudioService = Current.AddModule<AudioManager>();
            FXService = Current.AddModule<FXManager>();
            EventService = Current.AddModule<EventManager>();
            CameraService = Current.AddModule<CameraManager>();
            TimerService = Current.AddModule<TimerManager>();
            ProcedureService = Current.AddModule<ProcedureManager>();
            ProcedureService.SetProcedureHelper(new DefaultProcedureHelper());
            WindowService = Current.AddModule<WindowManager>();
            ThreadSyncService = Current.AddModule<ThreadSyncManager>();
            NetworkService = Current.AddModule<NetworkManager>();
            NetworkService.SetPackageHelper(new DefaultPackageHelper());
            NetworkService.SetMessageHelper(new DefaultMessageHelper());
            GizmoService = Current.AddModule<GizmoManager>();
            
            PlatformService = Current.AddModule<PlatformManager>();
            IconService = Current.AddModule<IconManager>();
            HotfixBridgeService = Current.AddModule<HotfixBridge>();
            ConfigService = Current.AddModule<ConfigManager>();
            LanguageService = Current.AddModule<LanguageManager>();
            WorldService = Current.AddModule<WorldManager>();
            BTScriptService = Current.AddModule<BTScriptManager>();
            UIService = Current.AddModule<UIManager>();
            UIService.SetUIHelper(new UIHelper());
            
            //最后初始化
            SaveService.Init();
            AudioService.Init();
            HotfixBridgeService.Init();
            LanguageService.Init();
            UIService.Init();
            WindowService.Init();
            ModelService.Init();
        }
    }

    internal partial class Main : Module
    {
        public static IPlatformService PlatformService { get; set; }
        public static IIconService IconService { get; set; }
        public static IHotfixBridgeService HotfixBridgeService { get; set; }
        public static IConfigService ConfigService { get; set; }
        public static ILanguageService LanguageService { get; set; }
        public static IWorldService WorldService { get; set; }
        public static IBTScriptService BTScriptService { get; set; }
        public static IUIService UIService { get; set; }
    }
}