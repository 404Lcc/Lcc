using System;
using YooAsset;

namespace LccModel
{
    public enum ESteps
    {
        None,
        Update,
        Done,
    }

    public class PatchOperation : GameAsyncOperation
    {
        private EventGroup _eventGroup = new EventGroup();
        private StateMachine _machine;
        private string _packageName;
        private ESteps _steps = ESteps.None;

        public PatchOperation(string packageName)
        {
            _packageName = packageName;

            // 创建状态机
            _machine = new StateMachine(this);
            _machine.AddNode<FsmStartSplash>();
            _machine.AddNode<FsmLoadGameConfig>();
            _machine.AddNode<FsmLoadLanguage>();
            _machine.AddNode<FsmRequestServer>();
            _machine.AddNode<FsmRequestNotice>();
            _machine.AddNode<FsmInitializePackage>();
            _machine.AddNode<FsmRequestPackageVersion>();
            _machine.AddNode<FsmUpdatePackageManifest>();
            _machine.AddNode<FsmCreateDownloader>();
            _machine.AddNode<FsmDownloadPackageFiles>();
            _machine.AddNode<FsmDownloadPackageOver>();
            _machine.AddNode<FsmClearCacheBundle>();
            _machine.AddNode<FsmStartGame>();

            _machine.SetBlackboardValue("PackageName", packageName);
        }

        protected override void OnStart()
        {
            _steps = ESteps.Update;
            _machine.Run<FsmStartSplash>();
        }

        protected override void OnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Update)
            {
                _machine.Update();
            }
        }

        protected override void OnAbort()
        {
        }

        public void SetFinish()
        {
            _steps = ESteps.Done;
            _eventGroup.RemoveAllListener();
            Status = EOperationStatus.Succeed;
        }
    }
}