using System.Collections;
using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 下载更新文件
    /// </summary>
    public class FsmDownloadPackageFiles : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        void IStateNode.OnEnter()
        {
            PatchEventDefine.PatchStatesChange.SendEventMessage("开始下载补丁文件！");
            Patch.Instance.StartCoroutine(BeginDownload());
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        private IEnumerator BeginDownload()
        {
            var downloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue("Downloader");
            downloader.OnDownloadErrorCallback = PatchEventDefine.WebFileDownloadFailed.SendEventMessage;
            downloader.OnDownloadProgressCallback = PatchEventDefine.DownloadProgressUpdate.SendEventMessage;
            downloader.BeginDownload();
            yield return downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                yield break;

            _machine.ChangeState<FsmDownloadPackageOver>();
        }
    }
}