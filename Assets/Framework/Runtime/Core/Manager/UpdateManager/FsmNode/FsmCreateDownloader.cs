using ET;
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
			UpdateEventDefine.PatchStatesChange.Publish("创建补丁下载器！");
			CreateDownloader().Coroutine();
		}
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}

		public async ETTask CreateDownloader()
		{
            ETTask task = UpdatePanel.Instance.UpdateLoadingPercent(80, 85);

            int downloadingMaxNum = 10;
			int failedTryAgain = 3;
			var downloader = YooAssets.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
			UpdateManager.Instance.Downloader = downloader;

			await task;

            if (downloader.TotalDownloadCount == 0)
			{
				Debug.Log("Not found any download files !");
				_machine.ChangeState<FsmDownloadOver>();
			}
			else
			{
				//A total of 10 files were found that need to be downloaded
				Debug.Log($"Found total {downloader.TotalDownloadCount} files that need download ！");

				// 发现新更新文件后，挂起流程系统
				// 注意：开发者需要在下载前检测磁盘空间不足
				int totalDownloadCount = downloader.TotalDownloadCount;
				long totalDownloadBytes = downloader.TotalDownloadBytes;
				UpdateEventDefine.FoundUpdateFiles.Publish(totalDownloadCount, totalDownloadBytes);
			}
		}
	}
}