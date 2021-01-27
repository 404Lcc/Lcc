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
        public static List<AssetBundleData> BuildAssetBundleData(List<AssetBundleRule> assetBundleRuleDict)
        {
            List<AssetBundleData> assetBundleDataList = new List<AssetBundleData>();
            List<string> assetNameList = new List<string>();
            foreach (AssetBundleRule item in assetBundleRuleDict)
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
                        assetBundleDataList.Add(new AssetBundleData($"{md5}.unity3d", string.Empty, uint.MinValue, new string[] { assetName }));
                    }
                }
                if (item.assetBundleRuleType == AssetBundleRuleType.Directory)
                {
                    DirectoryInfo[] directoryInfos = DirectoryUtil.GetDirectorys(new DirectoryInfo(item.path), new List<DirectoryInfo>() { new DirectoryInfo(item.path) });
                    if (directoryInfos.Length == 0) continue;
                    foreach (DirectoryInfo directoryInfo in directoryInfos)
                    {
                        foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                        {
                            assetNameList.Add(fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/"));
                        }
                        string assetName = directoryInfo.FullName.Substring(directoryInfo.FullName.IndexOf("Assets")).Replace("\\", "/");
                        string md5 = MD5Util.ComputeMD5(assetName);
                        assetBundleDataList.Add(new AssetBundleData($"{md5}.unity3d", string.Empty, uint.MinValue, assetNameList.ToArray()));
                    }
                }
            }
            return assetBundleDataList;
        }
        public static void BuildAssetBundle(AssetBundleSetting assetBundleSetting)
        {
            Dictionary<string, AssetBundleData> assetBundleDataDict = new Dictionary<string, AssetBundleData>();
            Dictionary<string, AssetBundleRuleType> assetBundleRuleTypeDict = new Dictionary<string, AssetBundleRuleType>();
            string path = DirectoryUtil.GetDirectoryPath($"{assetBundleSetting.outputPath}/{GetPlatformForAssetBundle(EditorUserBuildSettings.activeBuildTarget)}");
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
                    DirectoryInfo[] directoryInfos = DirectoryUtil.GetDirectorys(new DirectoryInfo(item.path), new List<DirectoryInfo>() { new DirectoryInfo(item.path) });
                    if (directoryInfos.Length == 0) continue;
                    foreach (DirectoryInfo directoryInfo in directoryInfos)
                    {
                        foreach (FileInfo fileInfo in directoryInfo.GetFiles())
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
                assetBundleDataDict.Add(Path.GetFileNameWithoutExtension(item.assetBundleName), item);
            }
            AssetBundleConfig assetBundleConfig = new AssetBundleConfig(assetBundleSetting.buildId, assetBundleDataDict, assetBundleRuleTypeDict);
            FileUtil.SaveAsset(path, "AssetBundleConfig.json", JsonUtil.ToJson(assetBundleConfig));
            AssetDatabase.Refresh();
        }
        public static string GetPlatformForAssetBundle(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                default:
                    return string.Empty;
            }
        }
    }
}