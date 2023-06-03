using ET;

namespace LccModel
{
	/// <summary>
	/// 清理未使用的缓存文件
	/// </summary>
	public class FsmClearCache : IStateNode
	{
		private StateMachine _machine;

		public void OnCreate(StateMachine machine)
		{
			_machine = machine;
		}
		public void OnEnter()
		{
			UpdateEventDefine.PatchStatesChange.Publish("清理未使用的缓存文件！");
			var package = YooAsset.YooAssets.GetPackage(UpdateManager.DefaultPackage);
			var operation = package.ClearUnusedCacheFilesAsync();
			operation.Completed += Operation_Completed;
		}
		public void OnUpdate()
		{
		}
		public void OnExit()
		{
		}

		private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
            _machine.ChangeState<FsmPatchDone>();
		}
	}
}