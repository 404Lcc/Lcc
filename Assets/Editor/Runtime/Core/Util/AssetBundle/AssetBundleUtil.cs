using LccModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public static class AssetBundleUtil
    {
        public static AssetBundleRule TagFileRule()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return new AssetBundleRule(path, AssetBundleRuleType.File);
        }
        public static AssetBundleRule TagDirectoryRule()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return new AssetBundleRule(path, AssetBundleRuleType.Directory);
        }
        public static List<AssetBundleData> BuildAssetBundleData(AssetBundleRule[] assetBundleRules)
        {
            List<AssetBundleData> assetBundleDataList = new List<AssetBundleData>();
            List<string> assetNameList = new List<string>();
            foreach (AssetBundleRule item in assetBundleRules)
            {
                if (item.assetBundleRuleType == AssetBundleRuleType.File)
                {
                    FileInfo[] fileInfos = FileUtil.GetFiles(new DirectoryInfo(item.path), new List<FileInfo>());
                    if (fileInfos.Length == 0) continue;
                    List<FileInfo> fileInfoList = (from fileInfo in fileInfos where !string.IsNullOrEmpty(Path.GetExtension(fileInfo.Name)) && Path.GetExtension(fileInfo.Name) != ".meta" select fileInfo).ToList();
                    foreach (FileInfo fileInfo in fileInfoList)
                    {
                        string assetName = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/");
                        string md5 = MD5Util.ComputeMD5(assetName);
                        assetBundleDataList.Add(new AssetBundleData($"{md5}.unity3d", string.Empty, uint.MinValue, long.MinValue, new string[] { assetName }));
                    }
                }
                if (item.assetBundleRuleType == AssetBundleRuleType.Directory)
                {
                    DirectoryInfo[] directoryInfos = DirectoryUtil.GetDirectorys(new DirectoryInfo(item.path), new List<DirectoryInfo>());
                    if (directoryInfos.Length == 0) continue;
                    foreach (DirectoryInfo directoryInfo in directoryInfos)
                    {
                        FileInfo[] fileInfos = directoryInfo.GetFiles();
                        if (fileInfos.Length == 0) continue;
                        List<FileInfo> fileInfoList = (from fileInfo in fileInfos where !string.IsNullOrEmpty(Path.GetExtension(fileInfo.Name)) && Path.GetExtension(fileInfo.Name) != ".meta" select fileInfo).ToList();
                        foreach (FileInfo fileInfo in fileInfoList)
                        {
                            assetNameList.Add(fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/"));
                        }
                        if (assetNameList.Count > 0)
                        {
                            string assetName = directoryInfo.FullName.Substring(directoryInfo.FullName.IndexOf("Assets")).Replace("\\", "/");
                            string md5 = MD5Util.ComputeMD5(assetName);
                            assetBundleDataList.Add(new AssetBundleData($"{md5}.unity3d", string.Empty, uint.MinValue, long.MinValue, assetNameList.ToArray()));
                            assetNameList.Clear();
                        }
                    }
                }
            }
            return assetBundleDataList;
        }
        public static void BuildAssetBundle(AssetBundleSetting assetBundleSetting)
        {
            Dictionary<string, AssetBundleData> assetBundleDataDict = new Dictionary<string, AssetBundleData>();
            Dictionary<string, AssetBundleRuleType> assetBundleRuleTypeDict = new Dictionary<string, AssetBundleRuleType>();
            string path = PathUtil.GetDataPath(assetBundleSetting.outputPath, GetPlatformForAssetBundle(EditorUserBuildSettings.activeBuildTarget));
            foreach (DirectoryInfo item in DirectoryUtil.GetDirectorys(new DirectoryInfo(path), new List<DirectoryInfo>()))
            {
                item.Delete();
            }
            foreach (FileInfo item in FileUtil.GetFiles(new DirectoryInfo(path), new List<FileInfo>()))
            {
                item.Delete();
            }
            List<AssetBundleBuild> assetBundleBuildList = new List<AssetBundleBuild>();
            foreach (AssetBundleRule item in assetBundleSetting.assetBundleRuleList)
            {
                if (item.assetBundleRuleType == AssetBundleRuleType.File)
                {
                    FileInfo[] fileInfos = FileUtil.GetFiles(new DirectoryInfo(item.path), new List<FileInfo>());
                    if (fileInfos.Length == 0) continue;
                    List<FileInfo> fileInfoList = (from fileInfo in fileInfos where !string.IsNullOrEmpty(Path.GetExtension(fileInfo.Name)) && Path.GetExtension(fileInfo.Name) != ".meta" select fileInfo).ToList();
                    foreach (FileInfo fileInfo in fileInfoList)
                    {
                        assetBundleRuleTypeDict.Add(fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/"), AssetBundleRuleType.File);
                    }
                }
                if (item.assetBundleRuleType == AssetBundleRuleType.Directory)
                {
                    DirectoryInfo[] directoryInfos = DirectoryUtil.GetDirectorys(new DirectoryInfo(item.path), new List<DirectoryInfo>());
                    if (directoryInfos.Length == 0) continue;
                    foreach (DirectoryInfo directoryInfo in directoryInfos)
                    {
                        FileInfo[] fileInfos = directoryInfo.GetFiles();
                        if (fileInfos.Length == 0) continue;
                        List<FileInfo> fileInfoList = (from fileInfo in fileInfos where !string.IsNullOrEmpty(Path.GetExtension(fileInfo.Name)) && Path.GetExtension(fileInfo.Name) != ".meta" select fileInfo).ToList();
                        foreach (FileInfo fileInfo in fileInfoList)
                        {
                            assetBundleRuleTypeDict.Add(fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/"), AssetBundleRuleType.Directory);
                        }
                    }
                }
            }
            foreach (AssetBundleData item in assetBundleSetting.assetBundleDataList)
            {
                assetBundleBuildList.Add(new AssetBundleBuild()
                {
                    assetBundleName = item.assetBundleName,
                    assetNames = item.assetNames,
                });
            }
            AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(path, assetBundleBuildList.ToArray(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
            foreach (AssetBundleData item in assetBundleSetting.assetBundleDataList)
            {
                item.assetBundleHash = assetBundleManifest.GetAssetBundleHash(item.assetBundleName).ToString();
                BuildPipeline.GetCRCForAssetBundle($"{path}/{item.assetBundleName}", out item.assetBundleCRC);
                item.fileSize = FileUtil.GetFileSize($"{path}/{item.assetBundleName}");
                assetBundleDataDict.Add(Path.GetFileNameWithoutExtension(item.assetBundleName), item);
            }
            AssetBundleConfig assetBundleConfig = new AssetBundleConfig(assetBundleSetting.buildId, assetBundleDataDict, assetBundleRuleTypeDict);
            FileUtil.SaveAsset(path, "AssetBundleConfig.json", JsonUtil.ToJson(assetBundleConfig));
            if (assetBundleSetting.isCopyStreamingAssets)
            {
                string copyPath = PathUtil.GetStreamingAssetsPath(LccConst.Res, GetPlatformForAssetBundle(EditorUserBuildSettings.activeBuildTarget));
                foreach (DirectoryInfo item in DirectoryUtil.GetDirectorys(new DirectoryInfo(copyPath), new List<DirectoryInfo>()))
                {
                    item.Delete();
                }
                foreach (FileInfo item in FileUtil.GetFiles(new DirectoryInfo(copyPath), new List<FileInfo>()))
                {
                    item.Delete();
                }
                foreach (FileInfo item in FileUtil.GetFiles(new DirectoryInfo(path), new List<FileInfo>()))
                {
                    if (Path.GetExtension(item.Name) == ".meta") continue;
                    File.Copy(item.FullName, $"{PathUtil.GetStreamingAssetsPath(LccConst.Res, GetPlatformForAssetBundle(EditorUserBuildSettings.activeBuildTarget))}/{item.Name}");
                }
            }
            AssetDatabase.Refresh();
        }
        public static string GetPlatformForAssetBundle(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "IOS";
                default:
                    return string.Empty;
            }
        }
    }
}