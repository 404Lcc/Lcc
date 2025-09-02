using System;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public enum GameState
    {
        Official,//正式
        Fetch,//提审
    }

    public class Launcher : SingletonMono<Launcher>
    {
        public GameControl GameControl { get; private set; } = new GameControl();
        public GameAction GameAction { get; private set; } = new GameAction();
        public GameConfig GameConfig { get; private set; } = new GameConfig();
        public GameLanguage GameLanguage { get; private set; } = new GameLanguage();
        public GameServerConfig GameServerConfig { get; private set; } = new GameServerConfig();
        public GameNotice GameNotice { get; private set; } = new GameNotice();
        public GameState GameState { set; get; } = GameState.Official;
        public Assembly HotfixAssembly { get; set; }

        public void Init()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    Debug.LogError(e.ExceptionObject.ToString());
                };
                
                GameControl.ChangeFPS();
                GameControl.SetGameSpeed(1);

                YooAssets.Initialize();
                Event.Initalize();
                YooAssets.SetOperationSystemMaxTimeSlice(30);

                DontDestroyOnLoad(this.gameObject);

                StartLauncher();
            }
            catch (Exception e)
            {
                Debug.LogError("Init Error:" + e.StackTrace);
            }

        }
        
        public string GetClientVersion()
        {
            var showApp = int.Parse(Application.version.Split('.')[0]);

            string clientVersion = string.Empty;
#if !UNITY_EDITOR
            clientVersion = "version " + showApp + "." + Launcher.Instance.GameConfig.appVersion + "." + Launcher.Instance.GameConfig.channel + "." + GameServerConfig.svrResVersion;
#else
            if (IsAuditServer() || !Launcher.Instance.GameConfig.checkResUpdate)
                clientVersion = "version " + showApp + "." + Launcher.Instance.GameConfig.appVersion + "." + Launcher.Instance.GameConfig.channel + "." + GameServerConfig.svrResVersion;
            else
                clientVersion = "version " + showApp + "." + Launcher.Instance.GameConfig.appVersion + "." + Launcher.Instance.GameConfig.channel + "." + Launcher.Instance.GameConfig.resVersion;
#endif
            return clientVersion;
        }

        public bool IsAuditServer()
        {
#if !UNITY_EDITOR
            if (Launcher.Instance.GameState == GameState.Official)
            {
                return false;
            }
            else
            {
                return true;

            }
#endif
            return false;
        }
        
        public void StartLoad()
        {
            ResPath.InitPath();
            LoadLocalConfig();
        }

        public void LoadLocalConfig()
        {
            UIForeGroundPanel.Instance.FadeIn(0, null, false, 1, false);

        }


        public void StartLauncher()
        {
            Debug.Log("开启启动流程...");

            EPlayMode playMode = EPlayMode.HostPlayMode;
            //不检测热更走本地资源
            if (!Launcher.Instance.GameConfig.checkResUpdate)
            {
                playMode = EPlayMode.OfflinePlayMode;
            }

            //提审包走本地资源
            if (Launcher.Instance.IsAuditServer())
            {
                playMode = EPlayMode.OfflinePlayMode;
            }

            if (Application.isEditor)
            {
                playMode = EPlayMode.EditorSimulateMode;

#if USE_ASSETBUNDLE
                playMode = EPlayMode.OfflinePlayMode;
#endif
            }

            // 开始补丁更新流程
            var operation = new PatchOperation("DefaultPackage", playMode);
            YooAssets.StartOperation(operation);
        }

        public void LoadFinish()
        {
            GameControl.ChangeFPS();
            UILoadingPanel.Instance.Hide();
        }
    }
}