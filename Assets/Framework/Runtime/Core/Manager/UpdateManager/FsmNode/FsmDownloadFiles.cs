using ET;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccModel
{
	/// <summary>
	/// 下载更新文件
	/// </summary>
	public class FsmDownloadFiles : IStateNode
	{
		private StateMachine _machine;

		public void OnCreate(StateMachine machine)
		{
			_machine = machine;
		}
		public void OnEnter()
		{
			UpdateEventDefine.PatchStatesChange.Publish("开始下载补丁文件！");
			BeginDownload().Coroutine();
		}
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}

		private async ETTask BeginDownload()
        {
            ETTask task = UpdatePanel.Instance.UpdateLoadingPercent(85, 90);

            var downloader = UpdateManager.Instance.Downloader;

			// 注册下载回调
			downloader.OnDownloadErrorCallback = UpdateEventDefine.WebFileDownloadFailed.Publish;
			downloader.OnDownloadProgressCallback = UpdateEventDefine.DownloadProgressUpdate.Publish;
			downloader.BeginDownload();

			await downloader.Task;

			await task;
			// 检测下载结果
			if (downloader.Status != EOperationStatus.Succeed) return;


			_machine.ChangeState<FsmPatchDone>();
		}
	}
}