using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class FsmCreateDownloader : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(8);
            CreateAllDownloader();
        }
        public override void ChangeToNextState()
        {
            base.ChangeToNextState();
            _machine.ChangeState<FsmDownloadPackageFiles>();
        }
        void CreateAllDownloader()
        {
            int totalDownloadCount = 0;
            long totalDownloadBytes = 0;
            foreach (var packageName in AssetConfig.BPackageList)
            {
                var downloader = CreateDownloader(packageName);
                totalDownloadCount += downloader.TotalDownloadCount;
                totalDownloadBytes += downloader.TotalDownloadBytes;
            }
            _machine.SetBlackboardValue("BV_TotalDownloadCount", totalDownloadCount);
            _machine.SetBlackboardValue("BV_TotalDownloadBytes", totalDownloadBytes);
            if (totalDownloadCount == 0)
            {
                Debug.LogWarning("[Launch] skip update patch");
                _machine.ChangeState<FsmStartGame>();
            }
            else
            {
                // TODO：检测磁盘空间剩余大于2.5倍totalDownloadBytes
                // TODO：检测网络状态
                LaunchEvent.ShowMessageBox.Broadcast(new UIPanelLaunch.MessageBoxParams
                {
                    Content = StringTable.Get("Hint.FoundPatchFiles", UIPanelLaunch.FormatBytes(totalDownloadBytes)),
                    btnOptionList = new List<UIPanelLaunch.MessageBoxOption> {
                        new ()
                        {
                            name = StringTable.Get("Op.Confirm"),
                            action = OnConfirmDownloadPatchFiles
                        }
                    },
                });
            }
        }
        
        ResourceDownloaderOperation CreateDownloader(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            var downloader = package.CreateResourceDownloader(AssetConfig.DownloadingMaxNum, AssetConfig.FailedTryAgain);
            _machine.SetBlackboardValue($"BV_Downloader_{packageName}", downloader);
            return downloader;
        }

        private void OnConfirmDownloadPatchFiles()
        {
            ChangeToNextState();
        }
    }
}