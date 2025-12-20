using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class FsmUpdatePackageManifest : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage(Launcher.Instance.GameLanguage.GetLanguage("msg_update_resource"));
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

            Debug.Log($"PackageVersion = {packageVersion}");

            yield return operation;


            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                PatchEventDefine.PackageManifestUpdateFailed.SendEventMessage();
                yield break;
            }
            else
            {
                _machine.ChangeState<FsmCreateDownloader>();
            }
        }
    }
}