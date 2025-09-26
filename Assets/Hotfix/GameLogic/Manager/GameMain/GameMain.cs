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
            CoroutineService = Current.AddModule<CoroutineManager>();
            CoroutineService.SetCoroutineHelper(new DefaultCoroutineHelper());
            NetworkService = Current.AddModule<NetworkManager>();
            NetworkService.SetPackageHelper(new DefaultPackageHelper());
            NetworkService.SetMessageHelper(new DefaultMessageHelper());
            NetworkService.SetMessageDispatcherHelper(new DefaultMessageDispatcherHelper());
            NetworkService.SetNetworkCallbackHelper(new DefaultNetworkCallbackHelper());
            ModelService = Current.AddModule<ModelManager>();
            SaveService = Current.AddModule<SaveManager>();
            SaveService.SetSaveHelper(new DefaultSaveHelper());
            SettingService = Current.AddModule<SettingManager>();
            AudioService = Current.AddModule<AudioManager>();
            VibrationService = Current.AddModule<VibrationManager>();
            FXService = Current.AddModule<FXManager>();
            EventService = Current.AddModule<EventManager>();
            CameraService = Current.AddModule<CameraManager>();
            TimerService = Current.AddModule<TimerManager>();
            ProcedureService = Current.AddModule<ProcedureManager>();
            ProcedureService.SetProcedureHelper(new DefaultProcedureHelper());
            WindowService = Current.AddModule<WindowManager>();
            ThreadSyncService = Current.AddModule<ThreadSyncManager>();
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
            MirrorService = Current.AddModule<MirrorManager>();
            MirrorService.SetTransportHelper(new SteamTransportHelper());
            MirrorService.SetServerMessageDispatcherHelper(new MirrorServerPBMessageDispatcherHelper());
            MirrorService.SetClientMessageDispatcherHelper(new PBMessageDispatcherHelper());
            MirrorService.SetMirrorCallbackHelper(new DefaultMirrorCallbackHelper());
            SteamService = Current.AddModule<SteamManager>();
            SteamLobbyService = Current.AddModule<SteamLobbyManager>();
            SteamLobbyService.SetLobbyCallbackHelper(new MirrorLobbyCallbackHelper());
            
            //最后初始化
            WindowService.Init();
            SaveService.Init();
            SettingService.Init();
            AudioService.Init();
            VibrationService.Init();
            HotfixBridgeService.Init();
            LanguageService.Init();
            UIService.Init();
            MirrorService.Init();
            SteamService.Init();
            ModelService.Init();
            
            SteamLobbyService.CreateLobby();
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
        public static IMirrorService MirrorService { get; set; }
        public static ISteamService SteamService { get; set; }
        public static ISteamLobbyService SteamLobbyService { get; set; }
    }
}