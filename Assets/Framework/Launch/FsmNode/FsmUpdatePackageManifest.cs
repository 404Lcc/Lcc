using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class FsmUpdatePackageManifest : FsmLaunchStateNode
    {
        string _lastError;
        public override void OnEnter()
        {
            base.OnEnter();
            _lastError = null;
            StartCoroutine(UpdateAllPackageManifest());
        }

        private IEnumerator UpdateAllPackageManifest()
        {
            foreach (var packageName in AssetConfig.BPackageList)
            {
                yield return UpdatePackageManifest(packageName);
                if (!string.IsNullOrEmpty(_lastError))
                {
                    Debug.LogError($"[Launch]FsmUpdatePackageManifest UpdatePackageManifest failed: {_lastError}");
                    LaunchEvent.ShowMessageBox.Broadcast(new UIPanelLaunch.MessageBoxParams
                    {
                        Content = StringTable.Get("Hint.UpdatePackageManifestFailed", _lastError),
                        btnOptionList = new List<UIPanelLaunch.MessageBoxOption> {
                            new ()
                            {
                                name = StringTable.Get("Op.Retry"),
                                action = OnRetry,
                            }
                        },
                    });
                    yield break;
                }
            }

            var defaultPackage = YooAssets.TryGetPackage(AssetConfig.DefaultPackageName);
            AppConfig.LocalPackageVersion = defaultPackage.GetPackageVersion();
            LaunchEvent.ShowVersion.Broadcast(AppConfig.GetVersionStr());
            
            ChangeToNextState();
        }

        private IEnumerator UpdatePackageManifest(string packageName)
        {
            string packageVersion = null;
            if (!PatchConfig.IsEnablePatcher)
            {
                packageVersion = (string)_machine.GetBlackboardValue("BV_PackageVersion_" + packageName);
                if (packageVersion == null)
                {
                    _lastError = "packageVersion is null";
                    yield break;
                }
            }
            else
            {
                packageVersion = PatchConfig.version.MaxVersion.ToString();
            }

            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                _lastError = $"{packageName}:{operation.Error}";
            }
        }
        private void OnRetry()
        {
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
    }
}