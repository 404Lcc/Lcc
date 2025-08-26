using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 更新资源清单
    /// </summary>
    public class FsmUpdatePackageManifest : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            PatchStatesChange.SendEventMessage(Launcher.Instance.GetLanguage("msg_update_resource"));
            Launcher.Instance.StartCoroutine(UpdateManifest());
        }
        public void OnUpdate()
        {
        }
        public void OnExit()
        {
        }

        private IEnumerator UpdateManifest()
        {
            UILoadingPanel.Instance.UpdateLoadingPercent(81, 85);

            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var packageVersion = (string)_machine.GetBlackboardValue("PackageVersion");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);

            Debug.Log($"FsmUpdateManifest UpdatePackageManifestAsync PackageVersion = {packageVersion}");

            yield return operation;


            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                PatchManifestUpdateFailed.SendEventMessage();
                yield break;
            }
            else
            {
                _machine.ChangeState<FsmCreateDownloader>();
            }
        }
    }
}