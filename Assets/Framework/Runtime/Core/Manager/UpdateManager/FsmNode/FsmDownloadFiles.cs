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
			UpdateManager.Instance.StartCoroutine(BeginDownload());
		}
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}

		private IEnumerator BeginDownload()
		{
			var downloader = UpdateManager.Instance.Downloader;

			// 注册下载回调
			downloader.OnDownloadErrorCallback = UpdateEventDefine.WebFileDownloadFailed.Publish;
			downloader.OnDownloadProgressCallback = UpdateEventDefine.DownloadProgressUpdate.Publish;
			downloader.BeginDownload();
			yield return downloader;

			// 检测下载结果
			if (downloader.Status != EOperationStatus.Succeed)
				yield break;

			_machine.ChangeState<FsmPatchDone>();
		}
	}
}