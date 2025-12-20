using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 流程更新完毕
    /// </summary>
    public class FsmStartGame : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
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
            var packageName = (string)_machine.GetBlackboardValue("PackageName");

            // 更新多语言
            string languageName = Launcher.Instance.GameLanguage.GetSelectedLanguage();
            yield return Launcher.Instance.GameLanguage.UpdateLanguage(languageName);

            UILoadingPanel.Instance.SetVersion(Launcher.Instance.GetClientVersion());

            UILoadingPanel.Instance.UpdateLoadingPercent(96, 100);

            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage(packageName);
            YooAssets.SetDefaultPackage(gamePackage);

            PatchEventDefine.PatchStepsChange.SendEventMessage(Launcher.Instance.GameLanguage.GetLanguage("msg_game_start"));

            bool haveHotfixAssembly = Launcher.Instance.HotfixAssembly != null;
            ResourceDownloaderOperation downloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue("Downloader");

            Debug.Log("是否加载过程序集了 haveHotfixAssembly=" + haveHotfixAssembly + "  " + "下载资源数量=" + downloader.TotalDownloadCount);
            if (haveHotfixAssembly)
            {
                //如果没下载过资源就直接用当前的程序集跑
                if (downloader.TotalDownloadCount == 0)
                {
                    Run();
                }
                else
                {
                    //下载过资源就得重启游戏了
                    UILoadingPanel.Instance.ShowMessageBox(Launcher.Instance.GameLanguage.GetLanguage("msg_need_restart"), RestartApplication);
                }
            }
            else
            {
                yield return LoadAndRun();
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

        private IEnumerator LoadAndRun()
        {
            yield return null;
#if HybridCLR
            var package = YooAssets.GetPackage("DefaultPackage");
            var aotHandle = package.LoadAssetAsync<TextAsset>("aot");
            yield return aotHandle;
            var aotTxt = aotHandle.AssetObject as TextAsset;
            var aotList = aotTxt.text.Split('|');
            HomologousImageMode mode = HomologousImageMode.SuperSet;

            Debug.Log($"LoadAndRun aotList Length = {aotList.Length}");
            foreach (var aotDllName in aotList)
            {
                if (string.IsNullOrEmpty(aotDllName))
                    continue;

                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                var aotAssetHandle = package.LoadAssetAsync(aotDllName);
                yield return aotAssetHandle;

                var aotAssetObj = aotAssetHandle.AssetObject as TextAsset;
                LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(aotAssetObj.bytes, mode);
                if (errorCode != LoadImageErrorCode.OK)
                {
                    Debug.LogError($"LoadMetadataForAOTAssembly failed : {errorCode}");
                }

                aotHandle.Release();
            }

            var handle = package.LoadAssetAsync<TextAsset>("Unity.Hotfix.dll");
            yield return handle;
            var assetObj = handle.AssetObject as TextAsset;
            Launcher.Instance.HotfixAssembly = Assembly.Load(assetObj.bytes);
            handle.Release();
#else
            Launcher.Instance.HotfixAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "Unity.Hotfix");
#endif

            Run();
        }

        private void Run()
        {
            if (Launcher.Instance.HotfixAssembly == null)
                return;
            AStaticMethod start = new MonoStaticMethod(Launcher.Instance.HotfixAssembly, "LccHotfix.Init", "Start");
            start.Run();

            var patchOperation = _machine.Owner as PatchOperation;
            patchOperation.SetFinish();
        }
    }
}