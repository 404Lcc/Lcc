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
            CodeTypesService.LoadTypes(new Assembly[] { Launcher.Instance.hotfixAssembly });
            GameObjectPoolService = Current.AddModule<GameObjectPoolManager>();
            GameObjectPoolService.SetLoader((location, root) => AssetService.LoadRes<GameObject>(root, location));
            EasingService = Current.AddModule<EasingManager>();
            CoroutineService = Current.AddModule<CoroutineManager>();
            CoroutineService.SetCoroutineHelper(new DefaultCoroutineHelper());
            ModelService = Current.AddModule<ModelManager>();
            SaveService = Current.AddModule<SaveManager>();
            SaveService.SetSaveHelper(new DefaultSaveHelper());
            FunctionOpenService = Current.AddModule<FunctionOpenManager>();
            FXService = Current.AddModule<FXManager>();
            EventService = Current.AddModule<EventManager>();
            CameraService = Current.AddModule<CameraManager>();
            TimerService = Current.AddModule<TimerManager>();
            AssetService = Current.AddModule<AssetManager>();
            ProcedureService = Current.AddModule<ProcedureManager>();
            
            IconService = Current.AddModule<IconManager>();
            HotfixBridgeService = Current.AddModule<HotfixBridge>();
            HotfixBridgeService.Init();
            SceneService = Current.AddModule<SceneManager>();
            ConfigService = Current.AddModule<ConfigManager>();
            LanguageService = Current.AddModule<LanguageManager>();
            WorldService = Current.AddModule<WorldManager>();
            BTScriptService = Current.AddModule<BTScriptManager>();
            WindowService = Current.AddModule<WindowManager>();
            WindowService.InitWindowManager();
        }
    }

    internal partial class Main : Module
    {
        public static IIconService IconService { get; set; }
        public static IHotfixBridgeService HotfixBridgeService { get; set; }
        public static ISceneService SceneService { get; set; }
        public static IConfigService ConfigService { get; set; }
        public static ILanguageService LanguageService { get; set; }
        public static IWorldService WorldService { get; set; }
        public static IBTScriptService BTScriptService { get; set; }
        public static IWindowService WindowService { get; set; }
    }
}