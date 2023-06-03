using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
	/// <summary>
	/// 流程更新完毕
	/// </summary>
	public class FsmPatchDone : IStateNode
	{
		public void OnCreate(StateMachine machine)
		{
		}
		public void OnEnter()
		{
			UpdateEventDefine.PatchStatesChange.Publish("开始游戏！");

			Event.Instance.RemoveListener(EventType.UserTryInitialize, UpdateManager.Instance);
			Event.Instance.RemoveListener(EventType.UserBeginDownloadWebFiles, UpdateManager.Instance);
			Event.Instance.RemoveListener(EventType.UserTryUpdatePackageVersion, UpdateManager.Instance);
			Event.Instance.RemoveListener(EventType.UserTryUpdatePatchManifest, UpdateManager.Instance);
			Event.Instance.RemoveListener(EventType.UserTryDownloadWebFiles, UpdateManager.Instance);

			Event.Instance.RemoveListener(EventType.InitializeFailed, UpdatePanel.Instance);
            Event.Instance.RemoveListener(EventType.PatchStatesChange, UpdatePanel.Instance);
            Event.Instance.RemoveListener(EventType.FoundUpdateFiles, UpdatePanel.Instance);
            Event.Instance.RemoveListener(EventType.DownloadProgressUpdate, UpdatePanel.Instance);
            Event.Instance.RemoveListener(EventType.PackageVersionUpdateFailed, UpdatePanel.Instance);
            Event.Instance.RemoveListener(EventType.PatchManifestUpdateFailed, UpdatePanel.Instance);
            Event.Instance.RemoveListener(EventType.WebFileDownloadFailed, UpdatePanel.Instance);

            Loader.Instance.Start(UpdateManager.Instance.globalConfig);
        }
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}
	}
}