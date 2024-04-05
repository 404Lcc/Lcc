using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 清理未使用的缓存文件
    /// </summary>
    public class FsmClearPackageCache : IStateNode
    {
        private StateMachine _machine;

        void IStateNode.OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        void IStateNode.OnEnter()
        {
            PatchEventDefine.PatchStatesChange.SendEventMessage("清理未使用的缓存文件！");
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearUnusedCacheFilesAsync();
            operation.Completed += Operation_Completed;
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }

        private void Operation_Completed(AsyncOperationBase obj)
        {
            _machine.ChangeState<FsmUpdaterDone>();
        }
    }
}