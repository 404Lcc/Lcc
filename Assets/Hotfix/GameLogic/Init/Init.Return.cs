using System;
using System.Collections;
using System.Collections.Generic;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public partial class Init
    {
        public static bool restartOver = false;
        
        /// <summary>
        /// 重新启动游戏
        /// </summary>
        public static void ReturnToStart()
        {
            //关闭所有协程，如果patchOperation的状态机在运行，这里会杀掉
            Main.CoroutineService.StopAllTypeCoroutines();
            Main.WindowService.ShowMaskBox(0xFF, false);
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
            Main.SceneService.CleanScene();
            //重启
            Restart();
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public static void ReturnToLogin()
        {
            if (Main.SceneService.CurState == SceneType.None || Main.SceneService.CurState == SceneType.Login)
            {
                ReturnToStart();
                return;
            }

            //关闭所有协程
            Main.CoroutineService.StopAllTypeCoroutines();
            Main.WindowService.ShowMaskBox(0xFF, false);
            //todo清理菊花界面
            //清理加载界面
            UILoadingPanel.Instance.Hide();

            //重置速度
            Launcher.Instance.SetGameSlow(false);
            Launcher.Instance.SetGameSpeed(1);
            Launcher.Instance.Resume();

            //清理上个玩家数据
            ClearLastUserData();

            Main.SceneService.ChangeScene(SceneType.Login);
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
            UILoadingPanel.Instance.SetStartLoadingBg();
            UILoadingPanel.Instance.Show(Launcher.Instance.GetLanguage("msg_retrieve_server_data"));
            UILoadingPanel.Instance.SetText(string.Empty);
            yield return new WaitForSeconds(0.1f);
            yield return Launcher.Instance.StartCoroutine(ReCheckVersionCoroutine());

            // 中途结束
            if (restartOver)
            {
                UILoadingPanel.Instance.UpdateLoadingPercent(91, 98);
                //切换场景
                Main.SceneService.ChangeScene(SceneType.Login);
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