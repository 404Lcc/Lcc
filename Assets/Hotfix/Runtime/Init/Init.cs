using LccModel;

namespace LccHotfix
{
    public class Init
    {
        public static void Start()
        {
            try
            {
                Loader.Instance.FixedUpdate += FixedUpdate;
                Loader.Instance.Update += Update;
                Loader.Instance.LateUpdate += LateUpdate;
                Loader.Instance.OnApplicationQuit += OnApplicationQuit;


                Game.AddSingleton<EventSystem>().InitType(Loader.Instance.GetHotfixTypeDict());
                Game.AddSingleton<Root>();


                Game.Scene.AddComponent<Manager>();

                Game.Scene.AddComponent<ModelManager>();
                Game.Scene.AddComponent<ArchiveManager>();
                Game.Scene.AddComponent<AudioManager>();
                Game.Scene.AddComponent<CommandManager>();
                Game.Scene.AddComponent<ConfigManager>();
                Game.Scene.AddComponent<GameSettingManager>();
                Game.Scene.AddComponent<GlobalManager>();
                Game.Scene.AddComponent<LanguageManager>();
                Game.Scene.AddComponent<PanelManager>();
                Game.Scene.AddComponent<SceneStateManager>();
                Game.Scene.AddComponent<UIEventManager>();
                Game.Scene.AddComponent<VideoManager>();

                EventSystem.Instance.Publish(new Start());
            }
            catch (System.Exception e)
            {
                LogUtil.Error(e);
            }
        }
        private static void FixedUpdate()
        {
            Game.FixedUpdate();
        }
        private static void Update()
        {
            Game.Update();
            Game.FrameFinishUpdate();
        }
        private static void LateUpdate()
        {
            Game.LateUpdate();
        }
        private static void OnApplicationQuit()
        {
            Loader.Instance.FixedUpdate -= FixedUpdate;
            Loader.Instance.Update -= Update;
            Loader.Instance.LateUpdate -= LateUpdate;
            Loader.Instance.OnApplicationQuit -= OnApplicationQuit;
            Game.Close();
        }
    }
}