using ET;
using System;
using UnityEngine;

namespace LccModel
{
    public class Init : MonoBehaviour
    {
        public static int FPS_High = 60;
        public static int FPS_DEFAULT = 30;
        public static int FPS_PVE = 30;
        public static int FPS_LOADING = 15;

        public static bool gameStarted = false;
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                LogHelper.Error(e.ExceptionObject.ToString());
            };
            ETTask.ExceptionHandler += LogHelper.Error;

            var globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            Event.Initalize();

            Game.AddSingleton<Logger>();

            Game.AddSingleton<MainThreadSynchronizationContext>();
            Game.AddSingleton<Time>();
            Game.AddSingleton<Timer>();
            Game.AddSingleton<EventSystem>();
            Game.AddSingleton<Root>();
            Game.AddSingleton<Loader>();
            Game.AddSingleton<AssetManager>();

            Game.Scene.AddComponent<Manager>();

            Game.Scene.AddComponent<BridgeManager>();
            Game.Scene.AddComponent<CoroutineLockManager>();
            Game.Scene.AddComponent<DownloadManager>();
            Game.Scene.AddComponent<NumericEventManager>();
            Game.Scene.AddComponent<RedDotManager>();
            Game.Scene.AddComponent<PatchManager>();

            Game.Scene.AddComponent<ModelPoolManager>();
            Game.Scene.AddComponent<OrcaManager>();

            //Game.Scene.AddComponent<CombatScriptManager>();
            //Game.Scene.AddComponent<CombatContext>();
            //Game.Scene.AddComponent<CombatViewContext>();

            PatchManager.Instance.StartUpdate(globalConfig);

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


        #region 控制游戏

        public static bool isPause;
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public static void Pause()
        {
            if (!isPause)
            {
                UnityEngine.Time.timeScale = 0;
                isPause = true;
            }
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public static void Resume()
        {
            if (isPause)
            {
                UnityEngine.Time.timeScale = gameTimeScale * slowTimeScale;
                isPause = false;
            }
        }

        public static float standardDeltaTime = 0.033f;
        public static void ChangeFPS()
        {
            Application.targetFrameRate = FPS_DEFAULT;
            standardDeltaTime = 1f / Application.targetFrameRate;
        }

        public static void ChangeFPS(int value)
        {
            Application.targetFrameRate = value;
            standardDeltaTime = 1f / Application.targetFrameRate;
        }


        private static float gameTimeScale = 1f;
        public static void SetGameSpeed(float timeScale)
        {
            if (timeScale < 1)
            {
                Debug.LogError("SetGameSpeed = " + timeScale);
                return;
            }
            gameTimeScale = timeScale;
            UnityEngine.Time.timeScale = gameTimeScale * slowTimeScale;
        }
        private static float slowTimeScale = 1f;
        public static void SetGameSlow(bool slow, float timeScale = 1f)
        {
            if (slow)
            {
                if (timeScale > 1)
                {
                    Debug.LogError("SetGameSpeed = " + timeScale);
                    return;
                }
                slowTimeScale = timeScale;
            }
            else
            {
                slowTimeScale = 1f;
            }
            UnityEngine.Time.timeScale = gameTimeScale * slowTimeScale;
        }

        public static float GetGameTimeScale()
        {
            return gameTimeScale;
        }
        public static float GetSlowTimeScale()
        {
            return slowTimeScale;
        }

        #endregion
    }
}