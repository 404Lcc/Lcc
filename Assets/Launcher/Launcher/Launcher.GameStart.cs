using Entitas.VisualDebugging.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public bool restartOver = false;
        private Coroutine _coroutine;

        public GameState GameState { set; get; } = GameState.Official;

        public bool GameStarted { set; get; } = false;

        public Assembly hotfixAssembly;
        public readonly Dictionary<string, Type> hotfixTypeDict = new Dictionary<string, Type>();


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

                DebugSystems.avgResetInterval = AvgResetInterval.Never;

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
            StartCoroutine(LoadLocalConfig());
        }

        public IEnumerator LoadLocalConfig()
        {
            //初始化游戏配置
            yield return InitGameConfig();
            //初始化多语言
            yield return InitLanguage();

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
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(LoadCoroutine());
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
                Debug.Log($"初始化 需要重新下载安装包 GameConfig.appVersion:{GameConfig.appVersion}, mSvrVersion:{svrVersion}");
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

        private void StartDownloadUpdate()
        {
            //清理
            StopAllCoroutines();
            Event.ClearAll();
            YooAssets.SetDefaultPackage(null);
            YooAssets.RemovePackage(DefaultPackage);


            Debug.Log("Launcher 开启补丁更新流程...");
            PatchOperation patchOperation = new PatchOperation();
            YooAssets.StartOperation(patchOperation);
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




        #region 重启游戏
        public void Restart()
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(ReStartCoroutine());
        }
        private IEnumerator ReStartCoroutine()
        {
            restartOver = false;
            GameStarted = false;
            UIForeGroundPanel.Instance.FadeOut(0.3f);
            yield return null;
            GC.Collect();
            UILoadingPanel.Instance.ShowBG();
            UILoadingPanel.Instance.Show(GetLanguage("msg_retrieve_server_data"));
            UILoadingPanel.Instance.SetText(string.Empty);
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(ReCheckVersionCoroutine());

            // 中途结束
            if (restartOver)
            {
                UILoadingPanel.Instance.UpdateLoadingPercent(91, 98);
                //切换场景
                HotfixFunc.CallPublicStaticMethod("Hotfix", "GameUtil", "ChangeSceneById", 1 << 0, "UILogin");//Login =1 <<0,// 登录   1
                yield return null;
                LoadFinish();
            }
        }

        private IEnumerator ReCheckVersionCoroutine()
        {
            ChangeFPS();

            var isHotfixGameStartedObj = HotfixFunc.CallPublicStaticMethod("Hotfix", "GameUtil", "IsHotfixGameStarted");
            var isHotfixGameStarted = isHotfixGameStartedObj == null ? false : (bool)isHotfixGameStartedObj;

            UILoadingPanel.Instance.UpdateLoadingPercent(0, 18);
            //连接中心服，请求失败重新请求
            yield return StartCoroutine(RequestCenterServer());
            if (!requestCenterServerSucc)
            {
                //请求中心服如果失败了，直接断流程，清理下热更代码和资源，走初始化流程

                actionClose?.Invoke();
                yield return null;
                AssetManager.Instance.UnloadAllAssetsAsync();
                yield return null;

                yield break;
            }

            UILoadingPanel.Instance.UpdateLoadingPercent(19, 20);

            //检测是否需要重新下载安装包
            if (CheckIfAppShouldUpdate())
            {
                Debug.Log($"重启 需要重新下载安装包 GameConfig.appVersion:{GameConfig.appVersion}, mSvrVersion:{svrVersion}");
                ForceUpdate();
                yield break;
            }

            //读取Package的版本信息
            if (!Launcher.Instance.reCheckVersionUpdate && GameConfig.resVersion == Launcher.Instance.svrResVersion)
            {
                if (isHotfixGameStarted)
                {
                    restartOver = true;
                    yield break;
                }
            }

            //读取本地版本信息
            if (Launcher.GameConfig.checkResUpdate && !Launcher.Instance.IsAuditServer())
            {
                Launcher.GameConfig.AddConfig("resVersion", svrResVersion);
            }

            UILoadingPanel.Instance.UpdateLoadingPercent(21, 48);
            Launcher.Instance.reCheckVersionUpdate = false;


            actionClose?.Invoke();
            yield return null;
            AssetManager.Instance.UnloadAllAssetsAsync();
            yield return null;

            yield return new WaitForSeconds(0.3f);
            UILoadingPanel.Instance.UpdateLoadingPercent(49, 50);
            yield return null;


            StartDownloadUpdate();
        }

        public void LoadFinish()
        {
            GameStarted = true;
            ChangeFPS();
            _coroutine = null;
            UILoadingPanel.Instance.Hide();
        }

        #endregion

    }
}