using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class FsmDownloadPackageFiles : FsmLaunchStateNode
    {
        private int DownloadPackageFinishCount;
        private DownloadErrorData LastErrorData; 
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(9);
            DownloadPackageFinishCount = 0;
            StartCoroutine(DownloadAllPackage());
        }
        protected override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmClearCacheBundle>();
        }
        private IEnumerator DownloadAllPackage()
        {
            foreach (var packageName in AssetConfig.BPackageList)
            {
                yield return BeginDownload(packageName);
            }
            if (DownloadPackageFinishCount == AssetConfig.BPackageList.Count)
            {
                Debug.LogWarning("[Launch] FsmDownloadPackageFiles: download all package finish");
                
                var defaultPackage = YooAssets.TryGetPackage(AssetConfig.DefaultPackageName);
                GameConfig.LocalPackageVersion = defaultPackage.GetPackageVersion();
                LaunchEvent.ShowVersion.Broadcast(GameConfig.GetVersionStr());
                
                ChangeToNextState();
            }
            else
            {
                Debug.LogError($"[Launch] FsmDownloadPackageFiles: download package fail, last error:{DownloadErrorDataToString(LastErrorData)}");
                
                LaunchEvent.ShowMessageBox.Broadcast(new UIPanelLaunch.MessageBoxParams
                {
                    Content = StringTable.Get("Hint.DownloadPackageFilesFailed", DownloadErrorDataToString(LastErrorData)),
                    btnOptionList = new List<UIPanelLaunch.MessageBoxOption> {
                        new ()
                        {
                            name = StringTable.Get("Op.Retry"),
                            action = OnRetry,
                        }
                    },
                });
            }
        }

        private IEnumerator BeginDownload(string packageName)
        {
            var downloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue($"BV_Downloader_{packageName}");
            if (downloader == null || downloader.TotalDownloadCount == 0 || downloader.IsDone)
            {
                DownloadPackageFinishCount++;
                yield break;
            }
            downloader.DownloadErrorCallback = data =>
            {
                LastErrorData = data;
                Debug.LogError($"[Launch] FsmDownloadPackageFiles: download Error : {DownloadErrorDataToString(data)}");
            };
            downloader.DownloadUpdateCallback = data => {
                int totalDownloadCount = (int)_machine.GetBlackboardValue("BV_TotalDownloadCount");
                long totalDownloadBytes = (long)_machine.GetBlackboardValue("BV_TotalDownloadBytes");
                int downloadedCount = 0;
                long downloadedBytes = 0;
                foreach (var _packageName in AssetConfig.BPackageList)
                {
                    var _downloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue($"BV_Downloader_{_packageName}");
                    downloadedCount += _downloader.CurrentDownloadCount;
                    downloadedBytes += _downloader.CurrentDownloadBytes;
                }
                float progress = (float)downloadedBytes / totalDownloadBytes;
                string strProgress = $"{UIPanelLaunch.FormatBytes(downloadedBytes)}/{UIPanelLaunch.FormatBytes(totalDownloadBytes)}({downloadedCount}/{totalDownloadCount})";
                LaunchEvent.ShowProgress.Broadcast(progress, strProgress);
            };
            downloader.BeginDownload();
            yield return downloader;
            
            if (downloader.Status == EOperationStatus.Succeed)
            {
                DownloadPackageFinishCount++;
            }
        }

        private string DownloadErrorDataToString(DownloadErrorData data)
        {
            return $"Package:{data.PackageName}, File:{data.FileName}, ErrorInfo:{data.ErrorInfo}";
        }
        
        private void OnRetry()
        {
            _machine.ChangeState<FsmDownloadPackageFiles>();
        }
    }
}