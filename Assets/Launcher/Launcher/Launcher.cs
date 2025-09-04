using System;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public enum GameState
    {
        Official, //正式
        Fetch, //提审
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
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => { Debug.LogError(e.ExceptionObject.ToString()); };

                YooAssets.Initialize();
                Event.Initalize();
                ResPath.InitPath();
                YooAssets.SetOperationSystemMaxTimeSlice(30);

                DontDestroyOnLoad(this.gameObject);

                StartLauncher();
            }
            catch (Exception e)
            {
                Debug.LogError("Init Error:" + e.StackTrace);
            }

        }

        public void StartLauncher()
        {
            Debug.Log("开启启动流程...");

            var operation = new PatchOperation("DefaultPackage");
            YooAssets.StartOperation(operation);
        }

        public void LauncherFinish()
        {
            Debug.Log("启动流程完成");
            UILoadingPanel.Instance.Hide();
        }

        public string GetClientVersion()
        {
            var showApp = int.Parse(Application.version.Split('.')[0]);

            string clientVersion = string.Empty;
#if !UNITY_EDITOR
            clientVersion = "version " + showApp + "." + GameConfig.appVersion + "." + GameConfig.channel + "." + GameServerConfig.svrResVersion;
#else
            if (IsAuditServer() || !GameConfig.checkResUpdate)
                clientVersion = "version " + showApp + "." + GameConfig.appVersion + "." + GameConfig.channel + "." + GameServerConfig.svrResVersion;
            else
                clientVersion = "version " + showApp + "." + GameConfig.appVersion + "." + GameConfig.channel + "." + GameConfig.resVersion;
#endif
            return clientVersion;
        }

        public bool IsAuditServer()
        {
#if !UNITY_EDITOR
            if (GameState == GameState.Official)
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
        
        public void FixedUpdate()
        {
            GameAction.ExecuteOnFixedUpdate();
        }
        
        public void Update()
        {
            GameAction.ExecuteOnUpdate();
        }
        
        public void LateUpdate()
        {
            GameAction.ExecuteOnLateUpdate();
        }
        
        public void OnApplicationQuit()
        {
            GameAction.ExecuteOnClose();
        }
        
        public void OnDrawGizmos()
        {
            GameAction.ExecuteOnDrawGizmos();
        }
    }
}