using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 创建文件下载器
    /// </summary>
    public class FsmCreateDownloader : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            PatchStatesChange.SendEventMessage(Launcher.Instance.GetLanguage("msg_patch_downloader"));
            Launcher.Instance.StartCoroutine(CreateDownloader());
        }
        public void OnUpdate()
        {
        }
        public void OnExit()
        {
        }

        public IEnumerator CreateDownloader()
        {
            UILoadingPanel.Instance.UpdateLoadingPercent(86, 90);

            yield return null;

            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            _machine.SetBlackboardValue("Downloader", downloader);

            _machine.SetBlackboardValue("TotalDownloadCount", downloader.TotalDownloadCount);
            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("FsmCreateDownloader Not found any download files !");
                _machine.ChangeState<FsmDownloadOver>();
            }
            else
            {
                //A total of 10 files were found that need to be downloaded
                Debug.Log($"FsmCreateDownloader Found total {downloader.TotalDownloadCount} files that need download ！");

                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
                FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);
            }
        }
    }
}