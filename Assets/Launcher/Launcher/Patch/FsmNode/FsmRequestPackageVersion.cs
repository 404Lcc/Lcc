using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 更新资源版本号
    /// </summary>
    public class FsmRequestPackageVersion : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            PatchStatesChange.SendEventMessage(Launcher.Instance.GetLanguage("msg_get_latest"));
            Launcher.Instance.StartCoroutine(GetStaticVersion());
        }
        public void OnUpdate()
        {
        }
        public void OnExit()
        {
        }

        private IEnumerator GetStaticVersion()
        {
            UILoadingPanel.Instance.UpdateLoadingPercent(76, 80);

            yield return new WaitForSecondsRealtime(0.5f);

            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning("FsmUpdateVersion Error=" + operation.Error);
                PackageVersionUpdateFailed.SendEventMessage();
            }
            else
            {
                Debug.Log($"Request package version : {operation.PackageVersion}");
                _machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                _machine.ChangeState<FsmUpdatePackageManifest>();
            }
        }
    }
}