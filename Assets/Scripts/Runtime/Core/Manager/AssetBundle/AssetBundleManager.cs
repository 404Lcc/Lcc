using System.Collections.Generic;

namespace LccModel
{
    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        public string url;
        public AssetBundleConfig localAssetBundleConfig;
        public void InitManager(string url)
        {
            this.url = url;
        }
        public void CheckLocalAsset()
        {
            byte[] bytes = FileUtil.GetAsset($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}/AssetBundleConfig.json");
            if (bytes != null)
            {
                //开始更新
                localAssetBundleConfig = JsonUtil.ToObject<AssetBundleConfig>(bytes.GetString());
                CheckUpdateAsset(null, string.Empty);
                return;
            }
            DownloadData downloadData = new DownloadData("AssetBundleConfig", "", $"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}/AssetBundleConfig.json");
            downloadData.complete = InitAssets;
            downloadData.error = CheckUpdateAsset;
            DownloadManager.Instance.DownloadAsync(downloadData);
        }
        public void InitAssets(DownloadData downloadData)
        {
            byte[] bytes = FileUtil.GetAsset($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}/AssetBundleConfig.json");
            localAssetBundleConfig = JsonUtil.ToObject<AssetBundleConfig>(bytes.GetString());
            List<DownloadData> downloadDataList = new List<DownloadData>();
            foreach (string item in localAssetBundleConfig.assetBundleDataDict.Keys)
            {
                DownloadData data = new DownloadData(item, "", $"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}/{item}.unity3d");
                downloadDataList.Add(data);
            }
            //开始初始化资源
            DownloadManager.Instance.DownloadAsync(downloadDataList.ToArray());
        }
        public void CheckUpdateAsset(DownloadData downloadData, string error)
        {

        }
    }
}