using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 更新资源版本号
    /// </summary>
    public class FsmUpdatePackageVersion : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        void IStateNode.OnEnter()
        {
            PatchEventDefine.PatchStatesChange.SendEventMessage("获取最新的资源版本 !");
            Patch.Instance.StartCoroutine(UpdatePackageVersion());
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        private IEnumerator UpdatePackageVersion()
        {
            LoadingPanel.Instance.UpdateLoadingPercent(50, 70);

            yield return new WaitForSecondsRealtime(0.5f);

            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                PatchEventDefine.PackageVersionUpdateFailed.SendEventMessage();
            }
            else
            {
                _machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                _machine.ChangeState<FsmUpdatePackageManifest>();
            }
        }
    }
}