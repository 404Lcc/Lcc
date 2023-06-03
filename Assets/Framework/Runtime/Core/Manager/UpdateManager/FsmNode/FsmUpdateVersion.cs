using ET;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccModel
{
	/// <summary>
	/// 更新资源版本号
	/// </summary>
	public class FsmUpdateVersion : IStateNode
	{
		private StateMachine _machine;

		public void OnCreate(StateMachine machine)
		{
			_machine = machine;
		}
		public void OnEnter()
		{
			UpdateEventDefine.PatchStatesChange.Publish("获取最新的资源版本 !");
			GetStaticVersion().Coroutine();
		}
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}

		private async ETTask GetStaticVersion()
		{
            ETTask task = UpdatePanel.Instance.UpdateLoadingPercent(50, 70);

            var package = YooAssets.GetPackage(UpdateManager.DefaultPackage);
			var operation = package.UpdatePackageVersionAsync();

			await operation.Task;

			await task;


            if (operation.Status == EOperationStatus.Succeed)
			{
				UpdateManager.Instance.PackageVersion = operation.PackageVersion;
				_machine.ChangeState<FsmUpdateManifest>();
			}
			else
			{
				Debug.LogWarning(operation.Error);
				UpdateEventDefine.PackageVersionUpdateFailed.Publish();
			}
		}
	}
}