using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LccModel
{
    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        public string url;
        public bool isDownload;
        public int copyCount;
        public int currentCopyCount;
        public int updateCount;
        public int currentUpdateCount;
        public int checkCount;
        public int currentCheckCount;
        public AssetBundleManifest assetBundleManifest;
        public AssetBundleConfig localAssetBundleConfig;
        public AssetBundleConfig serverAssetBundleConfig;
        public event Action<string> Message;
        public event Action<int, int> CopyProgress;
        public event Action<int, int> DownloadProgress;
        public event Action<int, int> CheckProgress;
        public event Action Complete;
        public event Action<DownloadData, string> Error;
        public Hashtable assetBundles = new Hashtable();
        public void InitManager(string url)
        {
            this.url = url;
            isDownload = url == string.Empty ? false : true;
        }
        public async ETTask InitAssets(Action<string> message = null, Action<int, int> copyProgress = null, Action<int, int> downloadProgress = null, Action<int, int> checkProgress = null, Action complete = null, Action<DownloadData, string> error = null)
        {
            Message = message;
            CopyProgress = copyProgress;
            DownloadProgress = downloadProgress;
            CheckProgress = checkProgress;
            Complete = complete;
            Error = error;
            if (File.Exists($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/AssetBundleConfig.json"))
            {
                byte[] bytes = FileUtil.GetAsset($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/AssetBundleConfig.json");
                localAssetBundleConfig = JsonUtil.ToObject<AssetBundleConfig>(bytes.GetString());
                //更新资源
                UpdateAssets();
                return;
            }
            else
            {
                byte[] bytes = await WebUtil.DownloadBytes($"{(Application.platform == RuntimePlatform.Android ? string.Empty : "file://")}{PathUtil.GetPath(PathType.StreamingAssetsPath, "Res", PathUtil.GetPlatformForAssetBundle())}/AssetBundleConfig.json");
                if (bytes == null)
                {
                    UpdateAssets();
                }
                else
                {
                    await InitAssets(bytes);
                }
            }
        }
        public async ETTask InitAssets(byte[] bytes)
        {
            Message?.Invoke("初始化资源");
            FileUtil.SaveAsset(PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle()), "AssetBundleConfig.json", bytes);
            localAssetBundleConfig = JsonUtil.ToObject<AssetBundleConfig>(bytes.GetString());
            List<string> keyList = new List<string>();
            keyList.Add(PathUtil.GetPlatformForAssetBundle());
            keyList.Add($"{PathUtil.GetPlatformForAssetBundle()}.manifest");
            foreach (string item in localAssetBundleConfig.assetBundleDataDict.Keys)
            {
                keyList.Add($"{item}.unity3d");
            }
            copyCount = keyList.Count;
            //开始拷贝资源
            await CopyAssets(keyList.ToArray());
        }
        public async ETTask CopyAssets(string[] keys)
        {
            foreach (string item in keys)
            {
                byte[] bytes = await WebUtil.DownloadBytes($"{(Application.platform == RuntimePlatform.Android ? string.Empty : "file://")}{PathUtil.GetPath(PathType.StreamingAssetsPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{item}");
                if (bytes != null)
                {
                    FileUtil.SaveAsset(PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle()), item, bytes);
                    currentCopyCount += 1;
                    Message?.Invoke($"初始化资源 {currentCopyCount} / {copyCount}");
                    CopyProgress?.Invoke(currentCopyCount, copyCount);
                }
            }
            //更新资源
            UpdateAssets();
        }
        public void UpdateAssets()
        {
            if (isDownload)
            {
                DownloadData downloadData = new DownloadData("ServerAssetBundleConfig", $"{url}/{PathUtil.GetPlatformForAssetBundle()}/AssetBundleConfig.json", $"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/ServerAssetBundleConfig.json");
                downloadData.Complete += UpdateAssets;
                downloadData.Error += Error;
                DownloadManager.Instance.DownloadAsync(downloadData);
            }
            else
            {
                CheckAssetsComplete();
            }
        }
        public void UpdateAssets(DownloadData downloadData)
        {
            Message?.Invoke("检测资源中");
            byte[] bytes = FileUtil.GetAsset($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/ServerAssetBundleConfig.json");
            serverAssetBundleConfig = JsonUtil.ToObject<AssetBundleConfig>(bytes.GetString());
            List<DownloadData> downloadDataList = new List<DownloadData>();
            if (localAssetBundleConfig == null)
            {
                foreach (string item in serverAssetBundleConfig.assetBundleDataDict.Keys)
                {
                    DownloadData data = new DownloadData(item, $"{url}/{PathUtil.GetPlatformForAssetBundle()}/{item}.unity3d", $"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{item}.unity3d");
                    data.Complete += DownloadComplete;
                    data.Error += Error;
                    downloadDataList.Add(data);
                }
            }
            else if (localAssetBundleConfig.version < serverAssetBundleConfig.version)
            {
                foreach (string item in ComputeUpdateAssets())
                {
                    DownloadData data = new DownloadData(item, $"{url}/{PathUtil.GetPlatformForAssetBundle()}/{item}.unity3d", $"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{item}.unity3d");
                    data.Complete += DownloadComplete;
                    data.Error += Error;
                    downloadDataList.Add(data);
                }
            }
            DownloadData data1 = new DownloadData(PathUtil.GetPlatformForAssetBundle(), $"{url}/{PathUtil.GetPlatformForAssetBundle()}/{PathUtil.GetPlatformForAssetBundle()}", $"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{PathUtil.GetPlatformForAssetBundle()}");
            data1.Complete += DownloadComplete;
            data1.Error += Error;
            DownloadData data2 = new DownloadData($"{PathUtil.GetPlatformForAssetBundle()}.manifest", $"{url}/{PathUtil.GetPlatformForAssetBundle()}/{PathUtil.GetPlatformForAssetBundle()}.manifest", $"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{PathUtil.GetPlatformForAssetBundle()}.manifest");
            data2.Complete += DownloadComplete;
            data2.Error += Error;
            downloadDataList.Add(data1);
            downloadDataList.Add(data2);
            updateCount = downloadDataList.Count;
            DownloadManager.Instance.DownloadAsync(downloadDataList.ToArray());
        }
        /// <summary>
        /// 计算更新资源列表
        /// </summary>
        /// <returns></returns>
        public string[] ComputeUpdateAssets()
        {
            List<string> keyList = new List<string>();
            foreach (KeyValuePair<string, AssetBundleData> item in serverAssetBundleConfig.assetBundleDataDict)
            {
                if (!localAssetBundleConfig.assetBundleDataDict.ContainsKey(item.Key))
                {
                    keyList.Add(item.Key);
                }
                if (item.Value.assetBundleHash != localAssetBundleConfig.assetBundleDataDict[item.Key].assetBundleHash)
                {
                    keyList.Add(item.Key);
                }
                if (item.Value.assetBundleCRC != localAssetBundleConfig.assetBundleDataDict[item.Key].assetBundleCRC)
                {
                    keyList.Add(item.Key);
                }
            }
            return keyList.Distinct().ToArray();
        }
        public void DownloadComplete(DownloadData downloadData)
        {
            currentUpdateCount += 1;
            Message?.Invoke($"下载资源中 {currentUpdateCount} / {updateCount}");
            DownloadProgress?.Invoke(currentUpdateCount, updateCount);
            if (currentUpdateCount == updateCount)
            {
                CheckAssetsComplete();
            }
        }
        public void CheckAssetsComplete()
        {
            Message?.Invoke("检测资源完整性");
            AssetBundle assetBundle = AssetBundle.LoadFromFile($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{PathUtil.GetPlatformForAssetBundle()}");
            assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            assetBundle.Unload(false);
            List<string> assetBundleNameList = new List<string>();
            checkCount = localAssetBundleConfig.assetBundleDataDict.Count;
            foreach (KeyValuePair<string, AssetBundleData> item in localAssetBundleConfig.assetBundleDataDict)
            {
                if (!File.Exists($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{item.Key}.unity3d"))
                {
                    assetBundleNameList.Add(item.Key);
                    continue;
                }
                else if (new FileInfo($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{item.Key}.unity3d").Length != item.Value.fileSize)
                {
                    assetBundleNameList.Add(item.Key);
                    continue;
                }
                else if (localAssetBundleConfig.assetBundleDataDict[item.Key].assetBundleHash != assetBundleManifest.GetAssetBundleHash($"{item.Key}.unity3d").ToString())
                {
                    assetBundleNameList.Add(item.Key);
                    continue;
                }
                currentCheckCount += 1;
                CheckProgress?.Invoke(currentCheckCount, checkCount);
            }
            if (assetBundleNameList.Count == 0)
            {
                if (isDownload)
                {
                    File.Move($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/ServerAssetBundleConfig.json", $"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/AssetBundleConfig.json");
                }
                Complete?.Invoke();
            }
            else
            {
                Error?.Invoke(null, "资源不完整");
            }
        }
        public AssetBundle LoadAsset(string assetName)
        {
            string assetBundleName;
            if (localAssetBundleConfig.assetBundleRuleTypeDict[assetName] == AssetBundleRuleType.File)
            {
                assetBundleName = $"{MD5Util.ComputeMD5(assetName)}.unity3d";
            }
            else
            {
                assetBundleName = $"{MD5Util.ComputeMD5(Path.GetDirectoryName(assetName).Replace("\\", "/"))}.unity3d";
            }
            if (assetBundles.ContainsKey(assetBundleName))
            {
                return (AssetBundle)assetBundles[assetBundleName];
            }
            else
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{assetBundleName}");
                string[] dependencies = assetBundleManifest.GetAllDependencies(assetBundleName);
                foreach (string item in dependencies)
                {
                    assetBundles.Add(item, AssetBundle.LoadFromFile($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res", PathUtil.GetPlatformForAssetBundle())}/{item}"));
                }
                assetBundles.Add(assetBundleName, assetBundle);
                return assetBundle;
            }
        }
        public void UnloadAsset(string assetName)
        {
            string assetBundleName = $"{MD5Util.ComputeMD5(assetName)}.unity3d";
            if (assetBundles.ContainsKey(assetName))
            {
                AssetBundle assetBundle = (AssetBundle)assetBundles[assetBundleName];
                assetBundle.Unload(true);
                assetBundles.Remove(assetBundle);
            }
        }
    }
}