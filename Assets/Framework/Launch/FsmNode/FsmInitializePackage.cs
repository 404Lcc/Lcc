using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class FsmInitializePackage : FsmLaunchStateNode
    {
        string _lastError;
        public override void OnEnter()
        {
            base.OnEnter();
            _lastError = null;
            StartCoroutine(InitAllPackage());
        }

        private IEnumerator InitAllPackage()
        {
            foreach (var packageName in AssetConfig.BPackageList)
            {
                yield return InitPackage(packageName);
                if (!string.IsNullOrEmpty(_lastError))
                {
                    Debug.LogError($"[Launch] FsmInitializePackage {_lastError}");
                    LaunchEvent.ShowMessageBox.Broadcast(new UIPanelLaunch.MessageBoxParams
                    {
                        Content = StringTable.Get("Hint.InitializePackageFailed", _lastError),
                        btnOptionList = new List<UIPanelLaunch.MessageBoxOption> {
                            new ()
                            {
                                name = StringTable.Get("Op.Retry"),
                                action = OnRetry,
                            }
                        },
                    });
                    yield break;
                }
                Debug.Log($"[Launch] FsmInitializePackage init package({packageName}) succeed");
            }
            
            var gamePackage = YooAssets.GetPackage(AssetConfig.DefaultPackageName);
            YooAssets.SetDefaultPackage(gamePackage);

            ChangeToNextState();
        }
        
        private IEnumerator InitPackage(string packageName)
        {
            var package = YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
            if (package.InitializeStatus == EOperationStatus.Succeed)
            {
                yield break;
            }
            
            EPlayMode playMode = AssetConfig.PlayMode;
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters
                {
                    EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot)
                };
                initializationOperation = package.InitializeAsync(createParameters);
            }
            else if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GameConfig.versionConfig.PatchesAddresses[0];
                string fallbackHostServer = PatchConfig.versionConfig.PatchesAddresses.Count > 1 ? PatchConfig.versionConfig.PatchesAddresses[1] : defaultHostServer;
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters
                {
                    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(),
                    CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices)
                };
                initializationOperation = package.InitializeAsync(createParameters);
            }
            else if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = package.InitializeAsync(createParameters);
            }
            
            yield return initializationOperation;
            
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                _lastError = $"{packageName}:{initializationOperation.Error}";
            }
        }
        
        private void OnRetry()
        {
            _machine.ChangeState<FsmInitializePackage>();
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
}