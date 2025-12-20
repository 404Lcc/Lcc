using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class SimpleLauncher : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(RunLaunch());
        }

        private IEnumerator RunLaunch()
        {
            YooAssets.Initialize();
            
            foreach (var packageName in AssetConfig.BPackageList)
            {
                var package = YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
                if (package.InitializeStatus == EOperationStatus.Succeed)
                {
                    continue;
                }
                
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters
                {
                    EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot)
                };
                InitializationOperation initializationOperation = package.InitializeAsync(createParameters);
                
                yield return initializationOperation;
                
                if (initializationOperation.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"[SimpleLauncher] {packageName}:{initializationOperation.Error}");
                    continue;
                }
                
                var requestPackageVersionOperation = package.RequestPackageVersionAsync();
                yield return requestPackageVersionOperation;
                
                if (requestPackageVersionOperation.Status != EOperationStatus.Succeed)
                {
                    Debug.LogWarning($"[SimpleLauncher] FsmRequestPackageVersion RequestPackageVersion: package[{packageName}], {requestPackageVersionOperation.Error}");
                }
                else
                {
                    Debug.LogWarning($"[SimpleLauncher] Request package version : package[{packageName}], {requestPackageVersionOperation.PackageVersion}");
                    
                    var updatePackageManifestOperation = package.UpdatePackageManifestAsync(requestPackageVersionOperation.PackageVersion);
                    yield return updatePackageManifestOperation;
                    
                    if (updatePackageManifestOperation.Status != EOperationStatus.Succeed)
                    {
                        Debug.LogError($"[SimpleLauncher] FsmRequestPackageVersion UpdatePackageManifest: package[{packageName}], {updatePackageManifestOperation.Error}");
                    }
                }
            }
            
            Assembly hotUpdateAss = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
            Type type = hotUpdateAss.GetType("HotUpdate.StartGame");
            type.GetMethod("SimpleStart").Invoke(null, null);
        }
    }
}