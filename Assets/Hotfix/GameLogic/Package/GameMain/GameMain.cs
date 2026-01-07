using System.Collections;
using System.Reflection;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    internal class GameMain : Main
    {
        public override void OnInstall()
        {
            Launcher.Instance.OnFixedUpdate += OnFixedUpdate;
            Launcher.Instance.OnUpdate += OnUpdate;
            Launcher.Instance.OnLateUpdate += OnLateUpdate;
            Launcher.Instance.OnClose += OnClose;
            Launcher.Instance.OnGizmos += OnGizmos;

            CodeTypesService = Current.AddModule<CodeTypesManager>();
            CodeTypesService.LoadTypes(new Assembly[] { GetType().Assembly });
            AssetService = Current.AddModule<AssetManager>();
            GameObjectPoolService = Current.AddModule<GameObjectPoolManager>();
            GameObjectPoolService.SetAsyncLoader((location, assetLoader, onComplete) =>
            {
                assetLoader.LoadAssetAsync<GameObject>(location, (handle) =>
                {
                    var prefab = handle.AssetObject as GameObject;
                    onComplete(location, prefab);
                });
            });
            CoroutineService = Current.AddModule<CoroutineManager>();
            CoroutineService.SetCoroutineHelper(new DefaultCoroutineHelper());
            NetworkService = Current.AddModule<NetworkManager>();
            NetworkService.SetPackageHelper(new DefaultPackageHelper());
            NetworkService.SetMessageHelper(new DefaultMessageHelper());
            NetworkService.SetMessageDispatcherHelper(new DefaultMessageDispatcherHelper());
            NetworkService.SetNetworkCallbackHelper(new DefaultNetworkCallbackHelper());
            ModelService = Current.AddModule<ModelManager>();
            ClientConfigService = Current.AddModule<ClientConfigService>();
            SaveService = Current.AddModule<SaveManager>();
            SaveService.SetSaveHelper(new DefaultSaveHelper());
            SettingService = Current.AddModule<SettingManager>();
            FxService = Current.AddModule<FxCacheManager>();
            AudioService = Current.AddModule<AudioManager>();
            VibrationService = Current.AddModule<VibrationManager>();
            ValueEventService = Current.AddModule<ValueEventManager>();
            CameraService = Current.AddModule<CameraManager>();
            TimerService = Current.AddModule<TimerManager>();
            ProcedureService = Current.AddModule<ProcedureManager>();
            ProcedureService.SetProcedureHelper(new DefaultProcedureHelper());
            AtlasService = Current.AddModule<AtlasManager>();
            UIService = Current.AddModule<UIManager>();
            ThreadSyncService = Current.AddModule<ThreadSyncManager>();
            GizmoService = Current.AddModule<GizmoManager>();
            BadgeService = Current.AddModule<BadgeManager>();
            CustomLogicService = Current.AddModule<CustomLogicManager>();
            CustomLogicService.SetRegister(new LogicCfgContainerRegister());

            ConfigService = Current.AddModule<ConfigManager>();
            PlatformService = Current.AddModule<PlatformManager>();
            IconService = Current.AddModule<IconManager>();
            HotfixBridgeService = Current.AddModule<HotfixBridge>();
            LanguageService = Current.AddModule<LanguageManager>();
            // WorldService = Current.AddModule<WorldManager>();
            // BTScriptService = Current.AddModule<BTScriptManager>();
            FishNetService = Current.AddModule<FishNetManager>();
            FishNetService.SetHelper(new SteamFishNetHelper());
            FishNetService.SetServerMessageDispatcherHelper(new FishNetServerPBMessageDispatcherHelper());
            FishNetService.SetClientMessageDispatcherHelper(new PBMessageDispatcherHelper());
            FishNetService.SetCallbackHelper(new DefaultFishNetCallbackHelper());
            MirrorService = Current.AddModule<MirrorManager>();
            MirrorService.SetHelper(new SteamMirrorHelper());
            MirrorService.SetServerMessageDispatcherHelper(new MirrorServerPBMessageDispatcherHelper());
            MirrorService.SetClientMessageDispatcherHelper(new PBMessageDispatcherHelper());
            MirrorService.SetCallbackHelper(new DefaultMirrorCallbackHelper());
            SteamService = Current.AddModule<SteamManager>();
            SteamLobbyService = Current.AddModule<SteamLobbyManager>();
            SteamLobbyService.SetLobbyCallbackHelper(new MirrorLobbyCallbackHelper());
        }

        public override IEnumerator OnInitialize()
        {
            ModelService.Init();
            SaveService.Init();
            SettingService.Init();
            AudioService.Init();
            VibrationService.Init();

            ConfigService.Init();
            while (!ConfigService.Initialized)
            {
                yield return null;
            }
            HotfixBridgeService.Init();
            LanguageService.Init();
            var uiRootAsset = AssetService.LoadAssetAsync<GameObject>("UIRoot");
            yield return uiRootAsset;
            var uiRoot = new UIRoot(GameObject.Instantiate(uiRootAsset.AssetHandle().AssetObject as GameObject));
            UIService.Init(uiRoot);
            FishNetService.Init();
            MirrorService.Init();
            SteamService.Init();

            try
            {
                SaveService.CreateSaveFile("default.sav");

                ProcedureService.ChangeProcedure(ProcedureType.Login.ToInt());
            }
            catch (System.Exception e)
            {
                Log.Error(e);
            }
        }

        private static void OnFixedUpdate()
        {
        }

        private static void OnUpdate()
        {
            Main.Current.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private static void OnLateUpdate()
        {
            Main.Current.LateUpdate();
        }

        private static void OnGizmos()
        {
            Main.GizmoService.OnGizmos();
        }

        private static void OnClose()
        {
            Launcher.Instance.OnFixedUpdate -= OnFixedUpdate;
            Launcher.Instance.OnUpdate -= OnUpdate;
            Launcher.Instance.OnLateUpdate -= OnLateUpdate;
            Launcher.Instance.OnClose -= OnClose;
            Launcher.Instance.OnGizmos -= OnGizmos;
            Current.Shutdown();
        }
    }

    internal partial class Main
    {
        public static IConfigService ConfigService { get; set; }
        public static IPlatformService PlatformService { get; set; }
        public static IIconService IconService { get; set; }
        public static IHotfixBridgeService HotfixBridgeService { get; set; }
        public static ILanguageService LanguageService { get; set; }
        // public static IWorldService WorldService { get; set; }
        // public static IBTScriptService BTScriptService { get; set; }
        public static IFishNetService FishNetService { get; set; }
        public static IMirrorService MirrorService { get; set; }
        public static ISteamService SteamService { get; set; }
        public static ISteamLobbyService SteamLobbyService { get; set; }
    }
}