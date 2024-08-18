using YooAsset;

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
            PatchStatesChange.SendEventMessage(Launcher.Instance.GetLanguage("msg_clean_cache"));
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearUnusedBundleFilesAsync();
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