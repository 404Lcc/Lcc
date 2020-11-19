using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LccModel
{
    public class AddressablesManager : Singleton<AddressablesManager>
    {
        public async void CheckForCatalogUpdates()
        {
            await Addressables.InitializeAsync().Task;
            AsyncOperationHandle<List<string>> handle = Addressables.CheckForCatalogUpdates(false);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                List<string> catalogList = handle.Result;
                if (catalogList != null && catalogList.Count > 0)
                {
                    UpdateCatalogs(catalogList);
                }
            }
            Addressables.Release(handle);
        }
        private async void UpdateCatalogs(List<string> catalogList)
        {
            AsyncOperationHandle<List<IResourceLocator>> handle = Addressables.UpdateCatalogs(catalogList, false);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                List<object> keyList = new List<object>();
                foreach (IResourceLocator item in handle.Result)
                {
                    keyList.AddRange(item.Keys);
                }
                StartCoroutine(Update(keyList));
            }
            Addressables.Release(handle);
        }
        private IEnumerator Update(List<object> keyList)
        {
            AsyncOperationHandle<long> downloadSizeHandle = Addressables.GetDownloadSizeAsync(keyList);
            yield return downloadSizeHandle;
            if (downloadSizeHandle.Status == AsyncOperationStatus.Succeeded && downloadSizeHandle.Result > 0)
            {
                AsyncOperationHandle downloadDependenciesHandle = Addressables.DownloadDependenciesAsync(keyList, Addressables.MergeMode.Union);
                yield return downloadDependenciesHandle;
                Addressables.Release(downloadDependenciesHandle);
            }
            Addressables.Release(downloadSizeHandle);
        }
    }
}