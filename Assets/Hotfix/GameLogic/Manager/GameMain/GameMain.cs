using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    internal class GameMain : Main, ICoroutine
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
            // AudioService = Current.AddModule<AudioManager>();
            VibrationService = Current.AddModule<VibrationManager>();
            // FXService = Current.AddModule<FXManager>();
            ValueEventService = Current.AddModule<ValueEventManager>();
            CameraService = Current.AddModule<CameraManager>();
            TimerService = Current.AddModule<TimerManager>();
            ProcedureService = Current.AddModule<ProcedureManager>();
            ProcedureService.SetProcedureHelper(new DefaultProcedureHelper());
            WindowService = Current.AddModule<WindowManager>();
            ThreadSyncService = Current.AddModule<ThreadSyncManager>();
            GizmoService = Current.AddModule<GizmoManager>();
            BadgeService = Current.AddModule<BadgeManager>();

            PlatformService = Current.AddModule<PlatformManager>();
            IconService = Current.AddModule<IconManager>();
            HotfixBridgeService = Current.AddModule<HotfixBridge>();
            ConfigService = Current.AddModule<ConfigManager>();
            LanguageService = Current.AddModule<LanguageManager>();
            WorldService = Current.AddModule<WorldManager>();
            BTScriptService = Current.AddModule<BTScriptManager>();
            // UIService = Current.AddModule<UIManager>();
            // UIService.SetUIHelper(new UIHelper());
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

            this.StartCoroutine(Initialize());
        }

        public override bool IsInstalled()
        {
            return ConfigService.Initialized;
        }

        public IEnumerator Initialize()
        {
            Launcher.Instance.GameAction.OnFixedUpdate += FixedUpdate;
            Launcher.Instance.GameAction.OnUpdate += Update;
            Launcher.Instance.GameAction.OnLateUpdate += LateUpdate;
            Launcher.Instance.GameAction.OnClose += Close;
            Launcher.Instance.GameAction.OnDrawGizmos += DrawGizmos;

            //最后初始化
            Main.ModelService.Init();
            Main.SaveService.Init();
            Main.SettingService.Init();
            //Main.AudioService.Init();
            Main.VibrationService.Init();
            Main.HotfixBridgeService.Init();
            Main.ConfigService.Init();
            Main.LanguageService.Init();
            //Main.UIService.Init();
            Main.FishNetService.Init();
            Main.MirrorService.Init();
            Main.SteamService.Init();

            while (!IsInstalled())
            {
                yield return 0;
            }

            try
            {
                Launcher.Instance.LauncherFinish();

                Main.SaveService.CreateSaveFile("default.sav");

                Main.ProcedureService.ChangeProcedure(ProcedureType.Login.ToInt());
            }
            catch (System.Exception e)
            {
                Log.Error(e);
            }
        }

        private static void FixedUpdate()
        {
        }

        private static void Update()
        {
            Main.Current.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private static void LateUpdate()
        {
            Main.Current.LateUpdate();
        }

        private static void DrawGizmos()
        {
            Main.GizmoService.OnDrawGizmos();
        }

        private static void Close()
        {
            Launcher.Instance.GameAction.OnFixedUpdate -= FixedUpdate;
            Launcher.Instance.GameAction.OnUpdate -= Update;
            Launcher.Instance.GameAction.OnLateUpdate -= LateUpdate;
            Launcher.Instance.GameAction.OnClose -= Close;
            Launcher.Instance.GameAction.OnDrawGizmos -= DrawGizmos;
            Main.Current.Shutdown();
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
        public static IFishNetService FishNetService { get; set; }
        public static IMirrorService MirrorService { get; set; }
        public static ISteamService SteamService { get; set; }
        public static ISteamLobbyService SteamLobbyService { get; set; }
    }
}