using ET;
using System;
using UnityEngine;

namespace LccModel
{
    public class Init : MonoBehaviour
    {
        public GlobalConfig globalConfig;
        async ETTask Start()
        {
            DontDestroyOnLoad(gameObject);

            globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                LogUtil.Error(e.ExceptionObject.ToString());
            };

            Game.AddSingleton<Logger>();

            Game.AddSingleton<MainThreadSynchronizationContext>();
            Game.AddSingleton<Time>();
            Game.AddSingleton<Timer>();
            Game.AddSingleton<EventSystem>();
            Game.AddSingleton<Root>();
            Game.AddSingleton<Loader>();

            Game.Scene.AddComponent<Manager>();

            Game.Scene.AddComponent<AssetManager>();
            Game.Scene.AddComponent<BridgeManager>();
            Game.Scene.AddComponent<CoroutineLockManager>();
            Game.Scene.AddComponent<DownloadManager>();
            Game.Scene.AddComponent<NumericEventManager>();
            Game.Scene.AddComponent<RedDotManager>();
            Game.Scene.AddComponent<SceneLoadManager>();
            Game.Scene.AddComponent<UpdateManager>();

            Game.Scene.AddComponent<CombatContext>();
            Game.Scene.AddComponent<CombatViewContext>();

            await UpdateManager.Instance.StartUpdate();


            Loader.Instance.Start(globalConfig);

        }
        void FixedUpdate()
        {
            Loader.Instance.FixedUpdate?.Invoke();
            Game.FixedUpdate();
        }
        void Update()
        {
            Loader.Instance.Update?.Invoke();
            Game.Update();
            Game.FrameFinishUpdate();
        }
        void LateUpdate()
        {
            Loader.Instance.LateUpdate?.Invoke();
            Game.LateUpdate();
        }
        void OnApplicationQuit()
        {
            Loader.Instance.OnApplicationQuit?.Invoke();
            Game.Close();
        }
    }
}