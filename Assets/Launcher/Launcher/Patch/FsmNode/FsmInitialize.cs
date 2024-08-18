using System.IO;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 初始化资源包
    /// </summary>
    public class FsmInitialize : IStateNode
    {
        private StateMachine _machine;

        public EPlayMode mPlayMode { set; get; } = EPlayMode.HostPlayMode;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }
        public void OnEnter()
        {
            PatchStatesChange.SendEventMessage(Launcher.Instance.GetLanguage("msg_init_resource"));
            Launcher.Instance.StartCoroutine(InitPackage());
        }
        public void OnUpdate()
        {
        }
        public void OnExit()
        {
        }

        private IEnumerator InitPackage()
        {
            UILoadingPanel.Instance.UpdateLoadingPercent(71, 75);


            //不检测热更走本地资源
            if (!Launcher.GameConfig.checkResUpdate)
            {
                mPlayMode = EPlayMode.OfflinePlayMode;
            }


            //提审包走本地资源
            if (Launcher.Instance.IsAuditServer())
            {
                mPlayMode = EPlayMode.OfflinePlayMode;
            }

            if (Application.isEditor)
            {
                mPlayMode = EPlayMode.EditorSimulateMode;

#if USE_ASSETBUNDLE
                mPlayMode = EPlayMode.OfflinePlayMode;
#endif
            }

            Debug.Log($"FsmInitialize mPlayMode = {mPlayMode}");

            // 创建默认的资源包
            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var buildPipeline = (string)_machine.GetBlackboardValue("BuildPipeline");
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
                package = YooAssets.CreatePackage(packageName);

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (mPlayMode == EPlayMode.EditorSimulateMode)
            {
                var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(buildPipeline, packageName);
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (mPlayMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (mPlayMode == EPlayMode.HostPlayMode)
            {
                Debug.Log($"FsmInitialize 初始化资源包 hostServerURL = {GetHostServerURL()}");
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (mPlayMode == EPlayMode.WebPlayMode)
            {
                var createParameters = new WebPlayModeParameters();
                createParameters.WebFileSystemParameters = FileSystemParameters.CreateDefaultWebFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            yield return initializationOperation;



            if (initializationOperation.Status == EOperationStatus.Succeed)
            {
                if (!Launcher.GameConfig.checkResUpdate)
                {
                    _machine.ChangeState<FsmPatchDone>();
                }
                else
                {
                    _machine.ChangeState<FsmUpdateVersion>();
                }
            }
            else
            {
                Debug.LogWarning($"{initializationOperation.Error}");
                InitializeFailed.SendEventMessage();
            }
        }

        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
        {
            var GameConfig = Launcher.GameConfig;

            string release = GameConfig.isRelease == true ? "Release" : "Dev";
            int appVersion = GameConfig.appVersion;
            int resVersion = GameConfig.resVersion;

            if (GameConfig.checkResUpdate)
            {
                appVersion = Launcher.Instance.mSvrVersion;
                resVersion = Launcher.Instance.mSvrResVersion;
            }

            var svrResourceServerUrl = Launcher.Instance.mSvrResourceServerUrl;
#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                return $"{svrResourceServerUrl}/{release}/Android/{GameConfig.channel}/{appVersion}/{resVersion}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                return $"{svrResourceServerUrl}/{release}/IOS/{GameConfig.channel}/{appVersion}/{resVersion}";
            else
                return $"{svrResourceServerUrl}/{release}/PC/{GameConfig.channel}/{appVersion}/{resVersion}";
#else
            if (Application.platform == RuntimePlatform.Android)
                return $"{svrResourceServerUrl}/{release}/Android/{GameConfig.channel}/{appVersion}/{resVersion}";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                return $"{svrResourceServerUrl}/{release}/IOS/{GameConfig.channel}/{appVersion}/{resVersion}";
            else
                return $"{svrResourceServerUrl}/{release}/PC/{GameConfig.channel}/{appVersion}/{resVersion}";
#endif
        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }
            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }
            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }
    }

    /// <summary>
    /// 资源文件解密流
    /// </summary>
    public class BundleStream : FileStream
    {
        public const byte KEY = 64;

        public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
        {
        }
        public BundleStream(string path, FileMode mode) : base(path, mode)
        {
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var index = base.Read(array, offset, count);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] ^= KEY;
            }
            return index;
        }
    }
}