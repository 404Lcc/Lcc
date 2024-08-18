using System;
using YooAsset;

namespace LccModel
{
    public class PatchOperation : GameAsyncOperation
    {
        private enum ESteps
        {
            None,
            Update,
            Done,
        }

        private readonly EventGroup _eventGroup = new EventGroup();
        private readonly StateMachine _machine;
        private ESteps _steps = ESteps.None;

        public PatchOperation(bool restart)
        {
            // 注册监听事件
            _eventGroup.AddListener<UserTryInitialize>(OnHandleEventMessage);
            _eventGroup.AddListener<UserBeginDownloadWebFiles>(OnHandleEventMessage);
            _eventGroup.AddListener<UserTryUpdatePackageVersion>(OnHandleEventMessage);
            _eventGroup.AddListener<UserTryUpdatePatchManifest>(OnHandleEventMessage);
            _eventGroup.AddListener<UserTryDownloadWebFiles>(OnHandleEventMessage);

            // 创建状态机
            _machine = new StateMachine(this);
            _machine.AddNode<FsmGetNotice>();
            _machine.AddNode<FsmPatchPrepare>();
            _machine.AddNode<FsmInitialize>();
            _machine.AddNode<FsmUpdateVersion>();
            _machine.AddNode<FsmUpdateManifest>();
            _machine.AddNode<FsmCreateDownloader>();
            _machine.AddNode<FsmDownloadFiles>();
            _machine.AddNode<FsmDownloadOver>();
            _machine.AddNode<FsmClearCache>();
            _machine.AddNode<FsmPatchDone>();

            _machine.SetBlackboardValue("PackageName", Launcher.DefaultPackage);
            _machine.SetBlackboardValue("BuildPipeline", EDefaultBuildPipeline.BuiltinBuildPipeline.ToString());
            _machine.SetBlackboardValue("Restart", restart);
            _machine.SetBlackboardValue("TotalDownloadCount", 0);
        }
        protected override void OnStart()
        {
            _steps = ESteps.Update;
            _machine.Run<FsmGetNotice>();
        }
        protected override void OnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Update)
            {
                _machine.Update();
                if (_machine.CurrentNode == typeof(FsmPatchDone).FullName)
                {
                    _eventGroup.RemoveAllListener();
                    Status = EOperationStatus.Succeed;
                    _steps = ESteps.Done;
                }
            }
        }
        protected override void OnAbort()
        {
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        private void OnHandleEventMessage(IEventMessage message)
        {
            if (message is UserTryInitialize)
            {
                _machine.ChangeState<FsmInitialize>();
            }
            else if (message is UserBeginDownloadWebFiles)
            {
                _machine.ChangeState<FsmDownloadFiles>();
            }
            else if (message is UserTryUpdatePackageVersion)
            {
                _machine.ChangeState<FsmUpdateVersion>();
            }
            else if (message is UserTryUpdatePatchManifest)
            {
                _machine.ChangeState<FsmUpdateManifest>();
            }
            else if (message is UserTryDownloadWebFiles)
            {
                _machine.ChangeState<FsmCreateDownloader>();
            }
            else
            {
                throw new NotImplementedException($"{message.GetType()}");
            }
        }
    }
}