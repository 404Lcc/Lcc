using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using UnityEngine.EventSystems;
using YooAsset;

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
#if HybridCLR && !UNITY_EDITOR
            var package = YooAssets.GetPackage(AssetConfig.DefaultPackageName);
            foreach (var aotDllName in AOTGenericReferences.PatchedAOTAssemblyList)
            {
                var aotHandle = package.LoadAssetAsync<TextAsset>(aotDllName);
                yield return aotHandle;
                var aotAssetObj = aotHandle.AssetObject as TextAsset;
                LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(aotAssetObj.bytes, HomologousImageMode.SuperSet);
                if (errorCode != LoadImageErrorCode.OK)
                {
                    Debug.LogError($"LoadMetadataForAOTAssembly failed : {errorCode}");
                }
                aotHandle.Release();
            }
            var handle = package.LoadAssetAsync<TextAsset>("Unity.Hotfix.dll");
            yield return handle;
            var assetObj = handle.AssetObject as TextAsset;
            Assembly hotfixAss = Assembly.Load(assetObj.bytes);
            handle.Release();
#else
            Assembly hotfixAss = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Unity.Hotfix");
#endif
            Type type = hotfixAss.GetType("LccHotfix.Init");
            type.GetMethod("Start").Invoke(null, null);

            yield return null;
            
            _launcherOperation.SetFinish();
            OnExit();
        }
    }
}