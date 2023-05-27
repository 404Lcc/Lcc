using YooAsset;

namespace LccModel
{
    public class UpdateManager : AObjectBase
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
    }
}