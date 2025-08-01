using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace LccModel
{
    /// <summary>
    /// 流程更新完毕
    /// </summary>
    public class FsmPatchDone : IStateNode
    {
        private StateMachine _machine;
        public void OnCreate(StateMachine machine)
        {
            _machine = machine;

        }
        public void OnEnter()
        {
            var patchOperation = _machine.Owner as PatchOperation;
            patchOperation.RemoveAllListener();
            Launcher.Instance.StartCoroutine(LoadInitialize());
        }
        public void OnUpdate()
        {
        }
        public void OnExit()
        {
        }

        //初始化配置文件
        public IEnumerator LoadInitialize()
        {
            // 更新多语言
            string languageName = Launcher.Instance.GetSelectedLanguage();
            yield return Launcher.Instance.UpdateLanguage(languageName);

            UILoadingPanel.Instance.SetText(Launcher.Instance.GetClientVersion());

            UILoadingPanel.Instance.UpdateLoadingPercent(96, 100);

            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage(Launcher.DefaultPackage);
            YooAssets.SetDefaultPackage(gamePackage);

            PatchStatesChange.SendEventMessage(Launcher.Instance.GetLanguage("msg_game_start"));

            bool haveHotfixAssembly = Launcher.Instance.hotfixAssembly != null;
            int totalDownloadCount = (int)_machine.GetBlackboardValue("TotalDownloadCount");
            Debug.Log("是否加载过程序集了 haveHotfixAssembly=" + haveHotfixAssembly + "  " + "下载资源数量=" + totalDownloadCount);
            if (haveHotfixAssembly)
            {
                //如果没下载过资源就直接用当前的程序集跑
                if (totalDownloadCount == 0)
                {
                    Run();
                }
                else
                {
                    //下载过资源就得重启游戏了
                    UILoadingPanel.Instance.ShowMessageBox(Launcher.Instance.GetLanguage("msg_need_restart"), RestartApplication);
                }
            }
            else
            {
                LoadAndRun();
            }
        }

        private void RestartApplication()
        {
            if (Application.isEditor)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }
            if (Application.platform == RuntimePlatform.Android)
            {
                //AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                //AndroidJavaObject mainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                //mainActivity.Call("RestartApplication", 0);
                //jc.Dispose();
                //mainActivity.Dispose();

                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
                    const int kIntent_FLAG_ACTIVITY_NEW_TASK = 0x10000000;

                    var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                    var intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);

                    intent.Call<AndroidJavaObject>("setFlags", kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
                    currentActivity.Call("startActivity", intent);
                    currentActivity.Call("finish");
                    var process = new AndroidJavaClass("android.os.Process");
                    int pid = process.CallStatic<int>("myPid");
                    process.CallStatic("killProcess", pid);
                }

            }
        }

        private void LoadAndRun()
        {
            GameObject loader = new GameObject("loader");

            TextAsset aotTxt = YooAssets.GetPackage("DefaultPackage").LoadAssetSync("aot", typeof(TextAsset)).AssetObject as TextAsset;

            var aotlist = aotTxt.text.Split('|');
            HomologousImageMode mode = HomologousImageMode.SuperSet;

            Debug.Log($"LoadAndRun aotlist Length={aotlist.Length}");
            foreach (var aotDllName in aotlist)
            {
                if (string.IsNullOrEmpty(aotDllName))
                    continue;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                var aotItem = YooAssets.GetPackage("DefaultPackage").LoadAssetSync(aotDllName, typeof(TextAsset)).AssetObject as TextAsset;

                var dllBytes = aotItem.bytes;
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly aotDllName={aotDllName}, ret={err}");
            }

            TextAsset assetDll = YooAssets.GetPackage("DefaultPackage").LoadAssetSync("Unity.Hotfix.dll", typeof(TextAsset)).AssetObject as TextAsset;
            TextAsset assetPdb = YooAssets.GetPackage("DefaultPackage").LoadAssetSync("Unity.Hotfix.pdb", typeof(TextAsset)).AssetObject as TextAsset;

            if (assetDll == null || assetPdb == null)
            {
                return;
            }

            //加个保底
            try
            {
                Launcher.Instance.hotfixAssembly = Assembly.Load(assetDll.bytes, assetPdb.bytes);
            }
            catch (Exception e)
            {
                Debug.LogError("加载程序集崩溃了 提示玩家重启游戏了" + e.ToString());
                UILoadingPanel.Instance.ShowMessageBox(Launcher.Instance.GetLanguage("msg_need_restart"), RestartApplication);
                return;
            }

            Object.Destroy(loader);

            Run();

        }
        private void Run()
        {
            if (Launcher.Instance.hotfixAssembly == null)
                return;
            AStaticMethod start = new MonoStaticMethod(Launcher.Instance.hotfixAssembly, "LccHotfix.Init", "Start");
            start.Run();
        }
    }
}