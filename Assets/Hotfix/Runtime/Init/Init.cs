using LccModel;
using System.Reflection;

namespace LccHotfix
{
    public class Init
    {
        public static void Start()
        {
            try
            {
                Launcher.Instance.ActionFixedUpdate += FixedUpdate;
                Launcher.Instance.ActionUpdate += Update;
                Launcher.Instance.ActionLateUpdate += LateUpdate;
                Launcher.Instance.ActionClose += Close;


                CodeTypesManager.Instance.LoadTypes(new Assembly[] { Launcher.Instance.hotfixAssembly });
                //Game.AddSingleton<Root>();


                //Game.Scene.AddComponent<Manager>();

                //Game.Scene.AddComponent<ModelManager>();
                //Game.Scene.AddComponent<AudioManager>();
                //Game.Scene.AddComponent<CommandManager>();
                //Game.Scene.AddComponent<ConfigManager>();
                //Game.Scene.AddComponent<GlobalManager>();
                //Game.Scene.AddComponent<LanguageManager>();
                //Game.Scene.AddComponent<PanelManager>();
                //Game.Scene.AddComponent<SceneStateManager>();
                //Game.Scene.AddComponent<JumpManager>();
                //Game.Scene.AddComponent<VideoManager>();

            }
            catch (System.Exception e)
            {
                Log.Error(e);
            }
        }
        private static void FixedUpdate()
        {
            //Game.FixedUpdate();
        }
        private static void Update()
        {
            //Game.Update();
        }
        private static void LateUpdate()
        {
            //Game.LateUpdate();
        }
        private static void Close()
        {
            Launcher.Instance.ActionFixedUpdate -= FixedUpdate;
            Launcher.Instance.ActionUpdate -= Update;
            Launcher.Instance.ActionLateUpdate -= LateUpdate;
            Launcher.Instance.ActionClose -= Close;
            //Game.Close();
        }
    }
}