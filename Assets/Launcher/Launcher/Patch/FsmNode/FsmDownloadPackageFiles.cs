using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 下载更新文件
    /// </summary>
    public class FsmDownloadPackageFiles : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            PatchStatesChange.SendEventMessage(Launcher.Instance.GetLanguage("msg_download_patch"));
            Launcher.Instance.StartCoroutine(BeginDownload());
        }
        public void OnUpdate()
        {
        }
        public void OnExit()
        {
        }

        private IEnumerator BeginDownload()
        {
            UILoadingPanel.Instance.UpdateLoadingPercent(91, 95);

            var downloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue("Downloader");

            // 注册下载回调
            downloader.DownloadErrorCallback = WebFileDownloadFailed.SendEventMessage;
            downloader.DownloadUpdateCallback = DownloadProgressUpdate.SendEventMessage;
            downloader.BeginDownload();

            yield return downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                yield break;


            _machine.ChangeState<FsmStartGame>();
        }
    }
}