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
			UpdateManager.Instance.StartCoroutine(GetStaticVersion());
		}
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}

		private IEnumerator GetStaticVersion()
		{
			yield return new WaitForSecondsRealtime(0.5f);

			var package = YooAssets.GetPackage(UpdateManager.DefaultPackage);
			var operation = package.UpdatePackageVersionAsync();
			yield return operation;

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