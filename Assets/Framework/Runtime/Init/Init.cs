using ET;
using System;
using System.Threading;
using UnityEngine;

namespace LccModel
{
    public class Init : MonoBehaviour
    {
        public GlobalConfig globalConfig;
        async ETTask Start()
        {
            globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                LogUtil.LogError(e.ExceptionObject.ToString());
            };

            SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
            DontDestroyOnLoad(gameObject);

            Game.AddSingleton<EventSystem>();
            Game.AddSingleton<Root>();
            Game.AddSingleton<Loader>();

            Game.Scene.AddComponent<Manager>();

            Game.Scene.AddComponent<AssetManager>();
            Game.Scene.AddComponent<BridgeManager>();
            Game.Scene.AddComponent<ClientNetworkManager>();
            Game.Scene.AddComponent<CoroutineLockManager>();
            Game.Scene.AddComponent<DownloadManager>();
            Game.Scene.AddComponent<NumericEventManager>();
            Game.Scene.AddComponent<RedDotManager>();
            Game.Scene.AddComponent<SceneLoadManager>();
            Game.Scene.AddComponent<TimeManager>();
            Game.Scene.AddComponent<TimerManager>();
            Game.Scene.AddComponent<UpdateManager>();

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