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

            var showApp = int.Parse(Application.version.Split('.')[0]);
            UILoadingPanel.Instance.SetText("version " + showApp + "." + Launcher.GameConfig.appVersion + "." + Launcher.GameConfig.channel + "." + Launcher.GameConfig.resVersion);

            UILoadingPanel.Instance.UpdateLoadingPercent(96, 100);

            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage(Launcher.DefaultPackage);
            YooAssets.SetDefaultPackage(gamePackage);

            PatchStatesChange.SendEventMessage(Launcher.Instance.GetLanguage("msg_game_start"));

            bool restart = (bool)_machine.GetBlackboardValue("Restart");
            int totalDownloadCount = (int)_machine.GetBlackboardValue("TotalDownloadCount");

            if (restart && totalDownloadCount > 0)
            {
                UILoadingPanel.Instance.ShowMessageBox(Launcher.Instance.GetLanguage("msg_need_restart"), RestartApplication);
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
            Assembly assembly = null;
            GameObject loader = new GameObject("loader");

            TextAsset aotTxt = AssetManager.Instance.LoadRes<TextAsset>(loader, "aot");

            var aotlist = aotTxt.text.Split('|');
            HomologousImageMode mode = HomologousImageMode.SuperSet;

            Debug.Log($"LoadAndRun aotlist Length = {aotlist.Length}");
            foreach (var aotDllName in aotlist)
            {
                if (string.IsNullOrEmpty(aotDllName))
                    continue;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                var aotItem = AssetManager.Instance.LoadRes<TextAsset>(loader, aotDllName);

                var dllBytes = aotItem.bytes;
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly aotDllName = {aotDllName} ret = {err}");
            }

            TextAsset assetDll = AssetManager.Instance.LoadRes<TextAsset>(loader, "Unity.Hotfix.dll");
            TextAsset assetPdb = AssetManager.Instance.LoadRes<TextAsset>(loader, "Unity.Hotfix.pdb");

            if (assetDll == null || assetPdb == null)
            {
                return;
            }
            assembly = Assembly.Load(assetDll.bytes, assetPdb.bytes);

            Object.Destroy(loader);

            var hotfixTypeDict = Launcher.Instance.HotfixTypeDict;
            foreach (Type type in assembly.GetTypes())
            {
                if (type.FullName != null)
                {
                    hotfixTypeDict[type.FullName] = type;
                }
            }

            AStaticMethod start = new MonoStaticMethod(assembly, "LccHotfix.Init", "Start");
            start.Run();
        }
    }
}