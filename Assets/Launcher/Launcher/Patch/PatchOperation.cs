using System;
using YooAsset;

namespace LccModel
{
    public class PatchOperation
    {
        private EventGroup _eventGroup = new EventGroup();
        private StateMachine _machine;

        public PatchOperation()
        {
        }

        public void Run()
        {
            RemoveAllListener();
            // 注册监听事件
            _eventGroup.AddListener<UserTryInitialize>(OnHandleEventMessage);
            _eventGroup.AddListener<UserBeginDownloadWebFiles>(OnHandleEventMessage);
            _eventGroup.AddListener<UserTryUpdatePackageVersion>(OnHandleEventMessage);
            _eventGroup.AddListener<UserTryUpdatePatchManifest>(OnHandleEventMessage);
            _eventGroup.AddListener<UserTryDownloadWebFiles>(OnHandleEventMessage);

            // 创建状态机
            _machine = new StateMachine(this);
            _machine.AddNode<FsmGetNotice>();
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
            _machine.SetBlackboardValue("TotalDownloadCount", 0);

#if Offline
            _machine.Run<FsmInitialize>();
#else
            _machine.Run<FsmGetNotice>();
#endif
        }

        public void RemoveAllListener()
        {
            _eventGroup.RemoveAllListener();
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