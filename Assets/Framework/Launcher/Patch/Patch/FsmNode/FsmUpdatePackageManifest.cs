using System.Collections;
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

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        void IStateNode.OnEnter()
        {
            PatchEventDefine.PatchStatesChange.SendEventMessage("更新资源清单！");
            Patch.Instance.StartCoroutine(UpdateManifest());
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        private IEnumerator UpdateManifest()
        {
            LoadingPanel.Instance.UpdateLoadingPercent(70, 80);

            yield return new WaitForSecondsRealtime(0.5f);

            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var packageVersion = (string)_machine.GetBlackboardValue("PackageVersion");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                PatchEventDefine.PatchManifestUpdateFailed.SendEventMessage();
                yield break;
            }
            else
            {
                _machine.ChangeState<FsmCreatePackageDownloader>();
            }
        }
    }
}