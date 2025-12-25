using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class FsmRequestPackageVersion : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(6);
            StartCoroutine(RequestPackageVersion());
        }
        protected override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
        private IEnumerator RequestPackageVersion()
        {
            if (GameConfig.IsEnablePatcher)
            {
                ChangeToNextState();
                yield break;
            }

            foreach (var packageName in AssetConfig.BPackageList)
            {
                yield return StartCoroutine(RequestPackageVersion(packageName));
            }
            ChangeToNextState();
        }

        private IEnumerator RequestPackageVersion(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning($"[Launch] FsmRequestPackageVersion RequestPackageVersion: package[{packageName}], {operation.Error}");
            }
            else
            {
                Debug.LogWarning($"[Launch] Request package version : package[{packageName}], {operation.PackageVersion}");
                _machine.SetBlackboardValue($"BV_PackageVersion_{packageName}", operation.PackageVersion);
            }
        }
    }
}