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
        
        private readonly StateMachine _machine;
        private ESteps _steps = ESteps.None;
        
        public GameAction GameAction { get; private set; } = new GameAction();
        public GameConfig GameConfig { get; private set; } = new GameConfig();
        public GameLanguage GameLanguage { get; private set; } = new GameLanguage();
        public GameServerConfig GameServerConfig { get; private set; } = new GameServerConfig();
        public Assembly HotfixAssembly { get; set; }

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
        }
        
        protected override void OnStart()
        {
            _steps = ESteps.Update;
            _machine.Run();
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected override void OnAbort()
        {
        }
        
        public void SetFinish()
        {
            _machine.Stop();
            Status = EOperationStatus.Succeed;
            _steps = ESteps.Done;
        }
    }
}