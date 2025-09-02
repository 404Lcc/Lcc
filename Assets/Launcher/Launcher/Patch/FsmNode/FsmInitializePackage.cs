using System.IO;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    /// <summary>
    /// 初始化资源包
    /// </summary>
    public class FsmInitializePackage : IStateNode
    {
        private StateMachine _machine;

        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
            PatchEventDefine.PatchStepsChange.SendEventMessage(Launcher.Instance.GameLanguage.GetLanguage("msg_init_resource"));
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

            var packageName = (string)_machine.GetBlackboardValue("PackageName");
            var playMode = (EPlayMode)_machine.GetBlackboardValue("PlayMode");

            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package != null)
            {
                yield return package.DestroyAsync();
                YooAssets.RemovePackage(packageName);
            }

            package = YooAssets.CreatePackage(packageName);

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
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
            if (playMode == EPlayMode.WebPlayMode)
            {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
                var createParameters = new WebPlayModeParameters();
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
                initializationOperation = package.InitializeAsync(createParameters);
#else
                var createParameters = new WebPlayModeParameters();
                createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
#endif
            }

            yield return initializationOperation;

            // 如果初始化失败弹出提示界面
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning($"{initializationOperation.Error}");
                PatchEventDefine.InitializeFailed.SendEventMessage();
            }
            else
            {
                _machine.ChangeState<FsmRequestPackageVersion>();
            }
        }

        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
        {
            var GameConfig = Launcher.Instance.GameConfig;

            string release = GameConfig.isRelease == true ? "Release" : "Dev";
            int appVersion = GameConfig.appVersion;
            int resVersion = GameConfig.resVersion;

            if (GameConfig.checkResUpdate)
            {
                appVersion = Launcher.Instance.GameServerConfig.svrVersion;
                resVersion = Launcher.Instance.GameServerConfig.svrResVersion;
            }

            var svrResourceServerUrl = Launcher.Instance.GameServerConfig.svrResourceServerUrl;
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