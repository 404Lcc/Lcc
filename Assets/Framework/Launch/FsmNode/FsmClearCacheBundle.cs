
using System.Collections;
using YooAsset;

namespace LccModel
{
    public class FsmClearCacheBundle : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(10);
            StartCoroutine(ClearCacheFiles());
        }
        protected override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmStartGame>();

        }
        private IEnumerator ClearCacheFiles()
        {
            foreach (var packageName in AssetConfig.BPackageList)
            {
                yield return ClearCacheFiles(packageName);
            }
            ChangeToNextState();
        }

        private IEnumerator ClearCacheFiles(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            yield return package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        }
    }
}