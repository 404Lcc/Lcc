using LccModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        public static Dictionary<string, AssetBundleData> BuildAssetBundleData(Dictionary<string, AssetBundleRule> assetBundleRuleDict)
        {
            Dictionary<string, AssetBundleData> assetBundleDataDict = new Dictionary<string, AssetBundleData>();
            List<string> assetNameList = new List<string>();
            foreach (AssetBundleRule item in assetBundleRuleDict.Values)
            {
                if (item.assetBundleRuleType == AssetBundleRuleType.File)
                {
                    FileInfo[] fileInfos = FileUtil.GetFiles(new DirectoryInfo(item.path), new List<FileInfo>());
                    if (fileInfos == null) continue;
                    List<FileInfo> fileInfoList = (from fileInfo in fileInfos where !string.IsNullOrEmpty(Path.GetExtension(fileInfo.Name)) && Path.GetExtension(fileInfo.Name) != ".meta" select fileInfo).ToList();
                    foreach (FileInfo fileInfo in fileInfoList)
                    {
                        string assetName = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/");
                        string md5 = MD5Util.ComputeMD5(assetName);
                        assetBundleDataDict.Add(md5, new AssetBundleData($"{md5}.unity3d", string.Empty, new string[] { assetName }));
                    }
                }
                if (item.assetBundleRuleType == AssetBundleRuleType.Directory)
                {
                    DirectoryInfo[] directoryInfos = DirectoryUtil.GetDirectorys(new DirectoryInfo(item.path), new List<DirectoryInfo>() { new DirectoryInfo(item.path) });
                    if (directoryInfos == null) continue;
                    foreach (DirectoryInfo directoryInfo in directoryInfos)
                    {
                        foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                        {
                            assetNameList.Add(fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/"));
                        }
                        string assetName = directoryInfo.FullName.Substring(directoryInfo.FullName.IndexOf("Assets")).Replace("\\", "/");
                        string md5 = MD5Util.ComputeMD5(assetName);
                        assetBundleDataDict.Add(md5, new AssetBundleData($"{md5}.unity3d", string.Empty, assetNameList.ToArray()));
                    }
                }
            }
            return assetBundleDataDict;
        }
        public static void BuildAssetBundle(AssetBundleSetting assetBundleSetting)
        {
            assetBundleSetting.assetBundleRuleTypeDict = new Dictionary<string, AssetBundleRuleType>();
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
            foreach (AssetBundleRule item in assetBundleSetting.assetBundleRuleDict.Values)
            {
                if (item.assetBundleRuleType == AssetBundleRuleType.File)
                {
                    FileInfo[] fileInfos = FileUtil.GetFiles(new DirectoryInfo(item.path), new List<FileInfo>());
                    if (fileInfos == null) continue;
                    List<FileInfo> fileInfoList = (from fileInfo in fileInfos where !string.IsNullOrEmpty(Path.GetExtension(fileInfo.Name)) && Path.GetExtension(fileInfo.Name) != ".meta" select fileInfo).ToList();
                    foreach (FileInfo fileInfo in fileInfoList)
                    {
                        assetBundleSetting.assetBundleRuleTypeDict.Add(fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/"), AssetBundleRuleType.File);
                    }
                }
                if (item.assetBundleRuleType == AssetBundleRuleType.Directory)
                {
                    DirectoryInfo[] directoryInfos = DirectoryUtil.GetDirectorys(new DirectoryInfo(item.path), new List<DirectoryInfo>() { new DirectoryInfo(item.path) });
                    if (directoryInfos == null) continue;
                    foreach (DirectoryInfo directoryInfo in directoryInfos)
                    {
                        foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                        {
                            assetBundleSetting.assetBundleRuleTypeDict.Add(fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets")).Replace("\\", "/"), AssetBundleRuleType.Directory);
                        }
                    }
                }
            }
            foreach (AssetBundleData item in assetBundleSetting.assetBundleDataDict.Values)
            {
                assetBundleBuildList.Add(new AssetBundleBuild()
                {
                    assetBundleName = item.assetBundleName,
                    assetNames = item.assetNames,
                });
            }
            AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(path, assetBundleBuildList.ToArray(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
            foreach (AssetBundleData item in assetBundleSetting.assetBundleDataDict.Values)
            {
                item.assetBundleHash = assetBundleManifest.GetAssetBundleHash(item.assetBundleName).ToString();
            }
            AssetBundleConfig assetBundleConfig = new AssetBundleConfig(assetBundleSetting.buildId, assetBundleSetting.assetBundleDataDict, assetBundleSetting.assetBundleRuleTypeDict);
            FileUtil.SaveAsset(path, $"{Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(nameof(assetBundleConfig))}.json", JsonUtil.ToJson(assetBundleConfig));
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