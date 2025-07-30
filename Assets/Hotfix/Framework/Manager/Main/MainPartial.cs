using System.Reflection;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    internal partial class Main : Module
    {
        public static Main Current { get; set; }

        public static ICodeTypesService CodeTypesService { get; set; }
        public static IGameObjectPoolService GameObjectPoolService { get; set; }
        public static IHotfixBridgeService HotfixBridgeService { get; set; }
        public static IWindowService WindowService { get; set; }
        public static ISceneService SceneService { get; set; }
        public static IModelService ModelService { get; set; }
        public static ISaveService SaveService { get; set; }
        public static ICoroutineService CoroutineService { get; set; }
        public static IConfigService ConfigService { get; set; }
        public static IEasingService EasingService { get; set; }
        public static IFunctionOpenService FunctionOpenService { get; set; }
        public static IIconService IconService { get; set; }
        public static IFXService FXService { get; set; }
        public static IEventService EventService { get; set; }
        public static ILanguageService LanguageService { get; set; }
        public static ICameraService CameraService { get; set; }
        public static ITimerService TimerService { get; set; }
        public static IWorldService WorldService { get; set; }
        public static IBTScriptService BTScriptService { get; set; }

        public static void SetMain(Main main)
        {
            if (main == null)
                return;
            Current = main;

            CodeTypesService = Current.AddModule<CodeTypesManager>();
            CodeTypesService.LoadTypes(new Assembly[] { Launcher.Instance.hotfixAssembly });

            GameObjectPoolService = Current.AddModule<GameObjectPoolManager>();
            GameObjectPoolService.SetLoader((location, root) => AssetManager.Instance.LoadRes<GameObject>(root, location));

            HotfixBridgeService = Current.AddModule<HotfixBridge>();
            HotfixBridgeService.Init();

            //初始化管理器
            WindowService = Current.AddModule<WindowManager>();
            WindowService.InitWindowManager();

            SceneService = Current.AddModule<SceneManager>();
            ModelService = Current.AddModule<ModelManager>();
            SaveService = Current.AddModule<SaveManager>();
            CoroutineService = Current.AddModule<CoroutineManager>();
            ConfigService = Current.AddModule<ConfigManager>();
            EasingService = Current.AddModule<EasingManager>();
            FunctionOpenService = Current.AddModule<FunctionOpenManager>();
            IconService = Current.AddModule<IconManager>();
            FXService = Current.AddModule<FXManager>();
            EventService = Current.AddModule<EventManager>();
            LanguageService = Current.AddModule<LanguageManager>();
            CameraService = Current.AddModule<CameraManager>();
            TimerService = Current.AddModule<TimerManager>();
            WorldService = Current.AddModule<WorldManager>();
            BTScriptService = Current.AddModule<BTScriptManager>();
            
            SceneService.ChangeScene(SceneType.Login);
        }
    }
}