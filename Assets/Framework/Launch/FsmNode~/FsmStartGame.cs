using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LccModel
{
    internal class FsmStartGame : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(11);
            StartCoroutine(LoadDlls());
        }
        public override void OnUpdate()
        {
        }
        public override void OnExit()
        {
            GameObject launchUI = (GameObject)_machine.GetBlackboardValue("BV_LaunchUI");
            if (launchUI)
            {
                // TODO：启动过程也在这个UI处理
                GameObject.Destroy(launchUI);
                _machine.SetBlackboardValue("BV_LaunchUI", null);
            }
        }

        private IEnumerator LoadDlls()
        {
#if !UNITY_EDITOR
            var package = YooAssets.GetPackage(AssetConfig.DefaultPackageName);
            foreach (var aotDllName in AOTGenericReferences.PatchedAOTAssemblyList)
            {
                var aotHandle = package.LoadAssetAsync<TextAsset>(aotDllName);
                yield return aotHandle;
                var aotAssetObj = aotHandle.AssetObject as TextAsset;
                LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(aotAssetObj.bytes, HomologousImageMode.SuperSet);
                if (errorCode != LoadImageErrorCode.OK)
                {
                    KLogger.LogError($"LoadMetadataForAOTAssembly failed : {errorCode}");
                }
                aotHandle.Release();
            }
            var handle = package.LoadAssetAsync<TextAsset>("HotUpdate.dll");
            yield return handle;
            var assetObj = handle.AssetObject as TextAsset;
            Assembly hotUpdateAss = Assembly.Load(assetObj.bytes);
            handle.Release();
#else
            Assembly hotUpdateAss = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#endif
            Type type = hotUpdateAss.GetType("HotUpdate.StartGame");
            type.GetMethod("Start").Invoke(null, null);

            yield return null;
            
            _launcherOperation.SetFinish();
        }
    }
}