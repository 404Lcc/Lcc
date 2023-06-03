using ET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
	/// <summary>
	/// 更新资源清单
	/// </summary>
	public class FsmUpdateManifest : IStateNode
	{
		private StateMachine _machine;

		public void OnCreate(StateMachine machine)
		{
			_machine = machine;
		}
		public void OnEnter()
		{
			UpdateEventDefine.PatchStatesChange.Publish("更新资源清单！");
			UpdateManifest().Coroutine();
		}
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}

		private async ETTask UpdateManifest()
        {
            ETTask task = UpdatePanel.Instance.UpdateLoadingPercent(70, 80);

            bool savePackageVersion = true;
			var package = YooAssets.GetPackage(UpdateManager.DefaultPackage);
			var operation = package.UpdatePackageManifestAsync(UpdateManager.Instance.PackageVersion, savePackageVersion);
			
			await operation.Task;
			await task;


            if (operation.Status == EOperationStatus.Succeed)
			{
				_machine.ChangeState<FsmCreateDownloader>();
			}
			else
			{
				Debug.LogWarning(operation.Error);
				UpdateEventDefine.PatchManifestUpdateFailed.Publish();
			}
		}
	}
}