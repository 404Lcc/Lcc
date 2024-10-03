using LccModel;
using System.Reflection;
using UnityEngine;

namespace LccHotfix
{
    public class Init
    {
        public static bool HotfixGameStarted { set; get; } = false;
        public static void Start()
        {
            HotfixGameStarted = false;
            try
            {
                Launcher.Instance.actionFixedUpdate += FixedUpdate;
                Launcher.Instance.actionUpdate += Update;
                Launcher.Instance.actionLateUpdate += LateUpdate;
                Launcher.Instance.actionClose += Close;
                Launcher.Instance.actionOnDrawGizmos += DrawGizmos;


                CodeTypesManager.Instance.LoadTypes(new Assembly[] { Launcher.Instance.hotfixAssembly });

                HotfixBridge.Init();

                if (Launcher.GameConfig.regionList != null)
                {
                    ReadUserRegion();
                }

                WindowManager.Instance.InitWindowManager();

                //SceneManager.Instance.GetScene(SceneType.Login).turnNode = new WNode.TurnNode();
                //SceneManager.Instance.ChangeScene(SceneType.Login);

                Launcher.Instance.LoadFinish();
                HotfixGameStarted = true;
            }
            catch (System.Exception e)
            {
                Log.Error(e);
            }
        }
        private static void FixedUpdate()
        {
            if (!Launcher.Instance.GameStarted)
                return;
        }
        private static void Update()
        {
            if (!Launcher.Instance.GameStarted)
                return;
            Entry.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
        private static void LateUpdate()
        {
            if (!Launcher.Instance.GameStarted)
                return;
        }
        private static void DrawGizmos()
        {
        }
        private static void Close()
        {
            Launcher.Instance.actionFixedUpdate -= FixedUpdate;
            Launcher.Instance.actionUpdate -= Update;
            Launcher.Instance.actionLateUpdate -= LateUpdate;
            Launcher.Instance.actionClose -= Close;
            Launcher.Instance.actionOnDrawGizmos -= DrawGizmos;

            Entry.Shutdown();
        }
        private static void ReadUserRegion()
        {
            string region = PlatformManager.Instance.GetUserRegion();

            var regions = Launcher.GameConfig.regionList;

            if (string.IsNullOrEmpty(region) || !regions.Contains(region))
            {
                region = "US";
            }
            Launcher.GameConfig.AddConfig("region", region);
        }
    }
}