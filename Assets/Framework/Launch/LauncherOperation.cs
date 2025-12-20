using System;
using System.Reflection;
using YooAsset;

namespace LccModel
{
    public class LauncherOperation : GameAsyncOperation
    {
        private enum ESteps
        {
            None,
            Update,
            Done,
        }

        private StateMachine _machine;
        private ESteps _steps = ESteps.None;

        public LauncherOperation()
        {
            _machine = new StateMachine(this);
            _machine.AddNode<FsmInitializeApp>();
            _machine.AddNode<FsmStartSplash>();
            _machine.AddNode<FsmShowLaunchUI>();
            _machine.AddNode<FsmRequestVersion>();
            _machine.AddNode<FsmInitializePackage>();
            _machine.AddNode<FsmRequestPackageVersion>();
            _machine.AddNode<FsmUpdatePackageManifest>();
            _machine.AddNode<FsmCreateDownloader>();
            _machine.AddNode<FsmDownloadPackageFiles>();
            _machine.AddNode<FsmClearCacheBundle>();
            _machine.AddNode<FsmStartGame>();
            _machine.SetBlackboardValue("total", 11);
        }

        protected override void OnStart()
        {
            _steps = ESteps.Update;
            _machine.Run<FsmInitializeApp>();
        }

        protected override void OnUpdate()
        {
            switch (_steps)
            {
                case ESteps.None:
                case ESteps.Done:
                    return;
                case ESteps.Update:
                    _machine.Update();
                    break;
            }
        }

        protected override void OnAbort()
        {
        }

        public void SetFinish()
        {
            Status = EOperationStatus.Succeed;
            _steps = ESteps.Done;
        }
    }
}