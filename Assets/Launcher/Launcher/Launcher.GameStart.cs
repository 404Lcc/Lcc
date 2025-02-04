using System;
using System.Collections;
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

    public partial class Launcher : SingletonMono<Launcher>
    {
        public Coroutine coroutine;

        public GameState GameState { set; get; } = GameState.Official;

        public bool GameStarted { set; get; } = false;

        public Assembly hotfixAssembly;
        public PatchOperation patchOperation = new PatchOperation();

        public const string DefaultPackage = "DefaultPackage";


        public void Init()
        {
            try
            {
                DateTime dt_1970 = new DateTime(1970, 1, 1);
                long tricks_1970 = dt_1970.Ticks;//1970年1月1日刻度    

                ChangeFPS();
                SetGameSpeed(1);

                YooAssets.Initialize();
                Event.Initalize();
                YooAssets.SetOperationSystemMaxTimeSlice(30);

                System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.GetCultureInfo("en-us");
                System.Threading.Thread.CurrentThread.CurrentCulture = cul;

                Input.multiTouchEnabled = false;

                Application.targetFrameRate = 60;

                DontDestroyOnLoad(this.gameObject);
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    Debug.LogError(e.ExceptionObject.ToString());
                };

                StartSplash();


            }
            catch (Exception e)
            {
                Debug.LogError("Init Error:" + e.StackTrace);
            }

        }



        public void StartSplash()
        {
            StartLoad();
        }
        public void StartLoad()
        {
            ResPath.InitPath();
            StartCoroutine(LoadLocalConfig());
        }

        public IEnumerator LoadLocalConfig()
        {
            UIForeGroundPanel.Instance.FadeIn(0, null, false, 1, false);

            //初始化游戏配置
            yield return InitGameConfig();
            //初始化多语言
            yield return InitLanguage();

            UIForeGroundPanel.Instance.FadeOut(0.5f, null, false);

            UILoadingPanel.Instance.SetStartLoadingBg();

            if (GameConfig.isReleaseCenterServer && GameConfig.isRelease)
            {
                //走sdk渠道号
                GameConfig.AddConfig("channel", GameConfig.channel);//todo 重新设置渠道 通过sdk获取
            }

            Debug.Log("Local GameConfig.appVersion:" + GameConfig.appVersion);
            Debug.Log("Local GameConfig.channel:" + GameConfig.channel);
            Debug.Log("Local GameConfig.resVersion:" + GameConfig.resVersion);
            Debug.Log("Local GameConfig.centerServerAddress:" + GameConfig.centerServerAddress);
            Debug.Log("Local GameConfig.isReleaseCenterServer:" + GameConfig.isReleaseCenterServer);
            Debug.Log("Local GameConfig.isRelease:" + GameConfig.isRelease);
            Debug.Log("Local GameConfig.chargeDirect:" + GameConfig.chargeDirect);
            Debug.Log("Local GameConfig.selectServer:" + GameConfig.selectServer);
            Debug.Log("Local GameConfig.checkResUpdate:" + GameConfig.checkResUpdate);
            Debug.Log("Local GameConfig.useSDK:" + GameConfig.useSDK);

            //本地版本
            var showApp = int.Parse(Application.version.Split('.')[0]);
            UILoadingPanel.Instance.SetText("version " + showApp + "." + Launcher.GameConfig.appVersion + "." + Launcher.GameConfig.channel + "." + Launcher.GameConfig.resVersion);


            StartServerLoad();
        }

        //出问题就走这个重来一遍
        public void StartServerLoad()
        {
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(LoadCoroutine());
        }


        private IEnumerator LoadCoroutine()
        {
            UILoadingPanel.Instance.Show(GetLanguage("msg_retrieve_server_data"));
            ChangeFPS();
            UILoadingPanel.Instance.UpdateLoadingPercent(0, 3);
            yield return null;

            UILoadingPanel.Instance.UpdateLoadingPercent(4, 20, 0.5f);
            yield return Launcher.Instance.RequestCenterServer();

            if (!Launcher.Instance.requestCenterServerSucc)
            {
                yield break;
            }

            UILoadingPanel.Instance.UpdateLoadingPercent(19, 20);

            //检测是否需要重新下载安装包
            if (CheckIfAppShouldUpdate())
            {
                Debug.Log($"初始化 需要重新下载安装包 GameConfig.appVersion:{GameConfig.appVersion}, svrVersion:{svrVersion}");
                ForceUpdate();
                yield break;
            }

            UILoadingPanel.Instance.UpdateLoadingPercent(21, 40);
            //读取本地版本信息
            if (GameConfig.checkResUpdate && !IsAuditServer())
            {
                Launcher.GameConfig.AddConfig("resVersion", svrResVersion);
            }
            UILoadingPanel.Instance.UpdateLoadingPercent(41, 50);
            yield return null;
            StartDownloadUpdate();
        }

        public void StartDownloadUpdate()
        {
            Debug.Log("Launcher 开启补丁更新流程...");
            patchOperation.Run();
        }

        public string GetPlatform()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                return "Android";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                return "IOS";
            else
                return "Android";
#else
            if (Application.platform == RuntimePlatform.Android)
                return "Android";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                return "IOS";
            else
                return "PC";
#endif

        }


        public void LoadFinish()
        {
            GameStarted = true;
            ChangeFPS();
            coroutine = null;
            UILoadingPanel.Instance.Hide();
        }



    }
}