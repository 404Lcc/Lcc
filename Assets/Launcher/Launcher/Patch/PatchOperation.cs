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
        private ESteps _steps = ESteps.None;

        public PatchOperation()
        {
            _eventGroup.RemoveAllListener();
            // 注册监听事件
            _eventGroup.AddListener<UserTryInitialize>(OnHandleEventMessage);
            _eventGroup.AddListener<UserBeginDownloadWebFiles>(OnHandleEventMessage);
            _eventGroup.AddListener<UserTryUpdatePackageVersion>(OnHandleEventMessage);
            _eventGroup.AddListener<UserTryUpdatePatchManifest>(OnHandleEventMessage);
            _eventGroup.AddListener<UserTryDownloadWebFiles>(OnHandleEventMessage);

            // 创建状态机
            _machine = new StateMachine(this);
            _machine.AddNode<FsmGetNotice>();
            _machine.AddNode<FsmInitializePackage>();
            _machine.AddNode<FsmRequestPackageVersion>();
            _machine.AddNode<FsmUpdatePackageManifest>();
            _machine.AddNode<FsmCreateDownloader>();
            _machine.AddNode<FsmDownloadPackageFiles>();
            _machine.AddNode<FsmDownloadPackageOver>();
            _machine.AddNode<FsmClearCacheBundle>();
            _machine.AddNode<FsmStartGame>();

            _machine.SetBlackboardValue("PackageName", Launcher.DefaultPackage);
            _machine.SetBlackboardValue("TotalDownloadCount", 0);
        }

        protected override void OnStart()
        {
            _steps = ESteps.Update;
#if Offline
            _machine.Run<FsmInitializePackage>();
#else
            _machine.Run<FsmGetNotice>();
#endif
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

        /// <summary>
        /// 接收事件
        /// </summary>
        private void OnHandleEventMessage(IEventMessage message)
        {
            if (message is UserTryInitialize)
            {
                _machine.ChangeState<FsmInitializePackage>();
            }
            else if (message is UserBeginDownloadWebFiles)
            {
                _machine.ChangeState<FsmDownloadPackageFiles>();
            }
            else if (message is UserTryUpdatePackageVersion)
            {
                _machine.ChangeState<FsmRequestPackageVersion>();
            }
            else if (message is UserTryUpdatePatchManifest)
            {
                _machine.ChangeState<FsmUpdatePackageManifest>();
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