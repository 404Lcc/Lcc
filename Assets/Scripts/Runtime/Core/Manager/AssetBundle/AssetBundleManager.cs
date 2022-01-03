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
        public const string AssetBundleConfig = "AssetBundleConfig.json";
        public const string ServerAssetBundleConfig = "ServerAssetBundleConfig.json";
        public const string AssetBundleSuffix = ".unity3d";
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
        public static string AssetBundleConfigPersistent
        {
            get
            {
                return $"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{AssetBundleConfig}";
            }
        }
        public static string AssetBundleConfigStreamingAssets
        {
            get
            {
                return $"{PathUtil.GetStreamingAssetsPathWeb(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{AssetBundleConfig}";
            }
        }
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
            if (File.Exists(AssetBundleConfigPersistent))
            {
                byte[] bytes = FileUtil.GetAsset(AssetBundleConfigPersistent);
                localAssetBundleConfig = JsonUtil.ToObject<AssetBundleConfig>(bytes.GetString());
                //更新资源
                UpdateAssets();
                return;
            }
            else
            {
                byte[] bytes = await WebUtil.DownloadBytes(AssetBundleConfigStreamingAssets);
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
            FileUtil.SaveAsset(AssetBundleConfigPersistent, bytes);
            localAssetBundleConfig = JsonUtil.ToObject<AssetBundleConfig>(bytes.GetString());
            List<string> keyList = new List<string>();
            keyList.Add(PathUtil.PlatformForAssetBundle);
            keyList.Add($"{PathUtil.PlatformForAssetBundle}.manifest");
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
                byte[] bytes = await WebUtil.DownloadBytes($"{PathUtil.GetStreamingAssetsPathWeb(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{item}");
                if (bytes != null)
                {
                    FileUtil.SaveAsset(PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle), item, bytes);
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
                DownloadData downloadData = new DownloadData(ServerAssetBundleConfig, $"{url}/{PathUtil.PlatformForAssetBundle}/{AssetBundleConfig}", $"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{ServerAssetBundleConfig}");
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
            byte[] bytes = FileUtil.GetAsset($"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{ServerAssetBundleConfig}");
            serverAssetBundleConfig = JsonUtil.ToObject<AssetBundleConfig>(bytes.GetString());
            List<DownloadData> downloadDataList = new List<DownloadData>();
            if (localAssetBundleConfig == null)
            {
                foreach (string item in serverAssetBundleConfig.assetBundleDataDict.Keys)
                {
                    DownloadData data = new DownloadData(item, $"{url}/{PathUtil.PlatformForAssetBundle}/{item}{AssetBundleSuffix}", $"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{item}{AssetBundleSuffix}");
                    data.Complete += DownloadComplete;
                    data.Error += Error;
                    downloadDataList.Add(data);
                }
            }
            else if (localAssetBundleConfig.version < serverAssetBundleConfig.version)
            {
                foreach (string item in ComputeUpdateAssets())
                {
                    DownloadData data = new DownloadData(item, $"{url}/{PathUtil.PlatformForAssetBundle}/{item}{AssetBundleSuffix}", $"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{item}{AssetBundleSuffix}");
                    data.Complete += DownloadComplete;
                    data.Error += Error;
                    downloadDataList.Add(data);
                }
            }
            DownloadData data1 = new DownloadData(PathUtil.PlatformForAssetBundle, $"{url}/{PathUtil.PlatformForAssetBundle}/{PathUtil.PlatformForAssetBundle}", $"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{PathUtil.PlatformForAssetBundle}");
            data1.Complete += DownloadComplete;
            data1.Error += Error;
            DownloadData data2 = new DownloadData($"{PathUtil.PlatformForAssetBundle}.manifest", $"{url}/{PathUtil.PlatformForAssetBundle}/{PathUtil.PlatformForAssetBundle}.manifest", $"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{PathUtil.PlatformForAssetBundle}.manifest");
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
            AssetBundle assetBundle = AssetBundle.LoadFromFile($"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{PathUtil.PlatformForAssetBundle}");
            assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            assetBundle.Unload(false);
            List<string> assetBundleNameList = new List<string>();
            checkCount = localAssetBundleConfig.assetBundleDataDict.Count;
            foreach (KeyValuePair<string, AssetBundleData> item in localAssetBundleConfig.assetBundleDataDict)
            {
                if (!File.Exists($"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{item.Key}{AssetBundleSuffix}"))
                {
                    assetBundleNameList.Add(item.Key);
                    continue;
                }
                else if (new FileInfo($"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{item.Key}{AssetBundleSuffix}").Length != item.Value.fileSize)
                {
                    assetBundleNameList.Add(item.Key);
                    continue;
                }
                else if (localAssetBundleConfig.assetBundleDataDict[item.Key].assetBundleHash != assetBundleManifest.GetAssetBundleHash($"{item.Key}{AssetBundleSuffix}").ToString())
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
                    File.Move($"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{serverAssetBundleConfig}", $"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{AssetBundleConfig}");
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
                assetBundleName = $"{MD5Util.ComputeMD5(assetName)}{AssetBundleSuffix}";
            }
            else
            {
                assetBundleName = $"{MD5Util.ComputeMD5(Path.GetDirectoryName(assetName).Replace("\\", "/"))}{AssetBundleSuffix}";
            }
            if (assetBundles.ContainsKey(assetBundleName))
            {
                return (AssetBundle)assetBundles[assetBundleName];
            }
            else
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile($"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{assetBundleName}");
                string[] dependencies = assetBundleManifest.GetAllDependencies(assetBundleName);
                foreach (string item in dependencies)
                {
                    assetBundles.Add(item, AssetBundle.LoadFromFile($"{PathUtil.GetPersistentDataPath(LccConst.Res, PathUtil.PlatformForAssetBundle)}/{item}"));
                }
                assetBundles.Add(assetBundleName, assetBundle);
                return assetBundle;
            }
        }
        public void UnloadAsset(string assetName)
        {
            string assetBundleName = $"{MD5Util.ComputeMD5(assetName)}{AssetBundleSuffix}";
            if (assetBundles.ContainsKey(assetName))
            {
                AssetBundle assetBundle = (AssetBundle)assetBundles[assetBundleName];
                assetBundle.Unload(true);
                assetBundles.Remove(assetBundle);
            }
        }
    }
}