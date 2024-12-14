using LccModel;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections;

namespace LccHotfix
{
    public class Init
    {
        public static bool restartOver = false;
        public static bool HotfixGameStarted { set; get; } = false;
        public static void Start()
        {
            HotfixGameStarted = false;
            Log.SetLogHelper(new DefaultLogHelper());
            try
            {
                Launcher.Instance.actionFixedUpdate += FixedUpdate;
                Launcher.Instance.actionUpdate += Update;
                Launcher.Instance.actionLateUpdate += LateUpdate;
                Launcher.Instance.actionClose += Close;
                Launcher.Instance.actionOnDrawGizmos += DrawGizmos;

                CodeTypesManager.Instance.LoadTypes(new Assembly[] { Launcher.Instance.hotfixAssembly });

                GameObjectPoolManager.Instance.SetLoader((location, root) => AssetManager.Instance.LoadRes<GameObject>(root, location));

                HotfixBridge.Init();

                //初始化管理器
                WindowManager.Instance.InitWindowManager();

                SceneManager.Instance.ChangeScene(SceneType.Login);

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
            Entry.LateUpdate();
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

            HotfixBridge.Dispose();
            Entry.Shutdown();
        }

        /// <summary>
        /// 重新启动游戏
        /// </summary>
        public static void ReturnToStart()
        {
            //关闭所有协程，如果patchOperation的状态机在运行，这里会杀掉
            CoroutineManager.Instance.StopAllTypeCoroutines();
            Entry.GetModule<WindowManager>().ShowMaskBox(0xFF, false);
            //todo清理菊花界面
            //清理加载界面
            UILoadingPanel.Instance.Hide();

            //重置速度
            Launcher.Instance.SetGameSlow(false);
            Launcher.Instance.SetGameSpeed(1);
            Launcher.Instance.Resume();

            //清理上个玩家数据
            ClearLastUserData();
            //清理场景
            SceneManager.Instance.CleanScene();
            //重启
            Restart();
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public static void ReturnToLogin()
        {
            if (SceneManager.Instance.curState == SceneType.None || SceneManager.Instance.curState == SceneType.Login)
            {
                ReturnToStart();
                return;
            }

            //关闭所有协程
            CoroutineManager.Instance.StopAllTypeCoroutines();
            Entry.GetModule<WindowManager>().ShowMaskBox(0xFF, false);
            //todo清理菊花界面
            //清理加载界面
            UILoadingPanel.Instance.Hide();

            //重置速度
            Launcher.Instance.SetGameSlow(false);
            Launcher.Instance.SetGameSpeed(1);
            Launcher.Instance.Resume();

            //清理上个玩家数据
            ClearLastUserData();

            SceneManager.Instance.ChangeScene(SceneType.Login);
        }

        private static void ClearLastUserData()
        {
        }

        #region 重启游戏
        public static void Restart()
        {
            if (Launcher.Instance.coroutine != null)
                Launcher.Instance.StopCoroutine(Launcher.Instance.coroutine);
            Launcher.Instance.coroutine = Launcher.Instance.StartCoroutine(ReStartCoroutine());
        }
        private static IEnumerator ReStartCoroutine()
        {
            restartOver = false;
            Launcher.Instance.GameStarted = false;
            UIForeGroundPanel.Instance.FadeOut(0.3f);
            yield return null;
            GC.Collect();
            UILoadingPanel.Instance.ShowBG();
            UILoadingPanel.Instance.Show(Launcher.Instance.GetLanguage("msg_retrieve_server_data"));
            UILoadingPanel.Instance.SetText(string.Empty);
            yield return new WaitForSeconds(0.1f);
            yield return Launcher.Instance.StartCoroutine(ReCheckVersionCoroutine());

            // 中途结束
            if (restartOver)
            {
                UILoadingPanel.Instance.UpdateLoadingPercent(91, 98);
                //切换场景
                SceneManager.Instance.ChangeScene(SceneType.Login);
                yield return null;
                Launcher.Instance.LoadFinish();
            }
        }

        private static IEnumerator ReCheckVersionCoroutine()
        {
            Launcher.Instance.ChangeFPS();

            UILoadingPanel.Instance.UpdateLoadingPercent(0, 18);
            //连接中心服，请求失败重新请求
            yield return Launcher.Instance.StartCoroutine(Launcher.Instance.RequestCenterServer(true));
            if (!Launcher.Instance.requestCenterServerSucc)
            {
                //请求中心服如果失败了
                restartOver = true;
                yield break;
            }

            UILoadingPanel.Instance.UpdateLoadingPercent(19, 20);

            //检测是否需要重新下载安装包
            if (Launcher.Instance.CheckIfAppShouldUpdate())
            {
                Debug.Log($"重启 需要重新下载安装包 GameConfig.appVersion:{Launcher.GameConfig.appVersion}, svrVersion:{Launcher.Instance.svrVersion}");
                Launcher.Instance.ForceUpdate();
                yield break;
            }

            //读取Package的版本信息
            if (!Launcher.Instance.reCheckVersionUpdate && Launcher.GameConfig.resVersion == Launcher.Instance.svrResVersion)
            {
                restartOver = true;
                yield break;
            }

            //读取本地版本信息
            if (Launcher.GameConfig.checkResUpdate && !Launcher.Instance.IsAuditServer())
            {
                Launcher.GameConfig.AddConfig("resVersion", Launcher.Instance.svrResVersion);
            }

            UILoadingPanel.Instance.UpdateLoadingPercent(21, 48);
            Launcher.Instance.reCheckVersionUpdate = false;


            Launcher.Instance.actionClose?.Invoke();
            yield return null;

            yield return new WaitForSeconds(0.3f);
            UILoadingPanel.Instance.UpdateLoadingPercent(49, 50);
            yield return null;


            Launcher.Instance.StartDownloadUpdate();
        }

        #endregion
    }
}