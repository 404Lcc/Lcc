using UnityEditor.VersionControl;
using YooAsset;

namespace LccModel
{
    public class UpdateManager : AObjectBase, IEventListener
    {
        public static UpdateManager Instance { get; set; }


        public EPlayMode PlayMode { private set; get; }
        public string PackageVersion { set; get; }
        public ResourceDownloaderOperation Downloader { set; get; }
        private bool _isRun = false;
        private StateMachine _machine;

        public GlobalConfig globalConfig;

        public const string DefaultPackage = "DefaultPackage";

        private string _version = "v1.0";

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Event.Instance.RemoveListener(EventType.UserTryInitialize, this);
            Event.Instance.RemoveListener(EventType.UserBeginDownloadWebFiles, this);
            Event.Instance.RemoveListener(EventType.UserTryUpdatePackageVersion, this);
            Event.Instance.RemoveListener(EventType.UserTryUpdatePatchManifest, this);
            Event.Instance.RemoveListener(EventType.UserTryDownloadWebFiles, this);

            Instance = null;
        }
        public void StartUpdate(GlobalConfig globalConfig)
        {
            if (_isRun == false)
            {
                YooAssets.Initialize();
                YooAssets.SetOperationSystemMaxTimeSlice(30);

                _isRun = true;
                this.globalConfig = globalConfig;
                PlayMode = globalConfig.playMode;

                _version = globalConfig.version;

                Event.Instance.AddListener(EventType.UserTryInitialize, this);
                Event.Instance.AddListener(EventType.UserBeginDownloadWebFiles, this);
                Event.Instance.AddListener(EventType.UserTryUpdatePackageVersion, this);
                Event.Instance.AddListener(EventType.UserTryUpdatePatchManifest, this);
                Event.Instance.AddListener(EventType.UserTryDownloadWebFiles, this);

                LogUtil.Debug("开启补丁更新流程...");
                _machine = new StateMachine(this);
                _machine.AddNode<FsmPatchPrepare>();
                _machine.AddNode<FsmInitialize>();
                _machine.AddNode<FsmUpdateVersion>();
                _machine.AddNode<FsmUpdateManifest>();
                _machine.AddNode<FsmCreateDownloader>();
                _machine.AddNode<FsmDownloadFiles>();
                _machine.AddNode<FsmDownloadOver>();
                _machine.AddNode<FsmClearCache>();
                _machine.AddNode<FsmPatchDone>();
                _machine.Run<FsmPatchPrepare>();
            }
            else
            {
                LogUtil.Warning("补丁更新已经正在进行中!");
            }
        }

        public string GetHostServer()
        {
            return globalConfig.hostServer;
        }
        public string GetVersion()
        {
            return _version;
        }

        public void HandleEvent(EventType eventType, IEventArgs args1 = null, IEventArgs args2 = null, IEventArgs args3 = null, IEventArgs args4 = null)
        {
            switch (eventType)
            {
                case EventType.UserTryInitialize:
                    _machine.ChangeState<FsmInitialize>();
                    break;
                case EventType.UserBeginDownloadWebFiles:
                    _machine.ChangeState<FsmDownloadFiles>();
                    break;
                case EventType.UserTryUpdatePackageVersion:
                    _machine.ChangeState<FsmUpdateVersion>();
                    break;
                case EventType.UserTryUpdatePatchManifest:
                    _machine.ChangeState<FsmUpdateManifest>();
                    break;
                case EventType.UserTryDownloadWebFiles:
                    _machine.ChangeState<FsmCreateDownloader>();
                    break;
            }
        }
    }
}