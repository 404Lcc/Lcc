using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public static class AppConfig
    {
        public static string AppLanguage = "zh_cn";
        public const string LocalizationSettingsNamePath = "Settings/localization";
        public const string DefaultLanguage = "zh_cn";

        public static readonly Dictionary<SystemLanguage, string> LanguageDict = new()
        {
            { SystemLanguage.Chinese, "zh_cn" },
            { SystemLanguage.ChineseSimplified, "zh_cn" },
            { SystemLanguage.ChineseTraditional, "zh_tw" },
            { SystemLanguage.English, "en" },
            { SystemLanguage.Vietnamese, "vn" },
        };
    }

    public static class StringTable
    {
        public static Dictionary<string, string> Strings = new();

        public static string Get(string key)
        {
            Strings.TryGetValue(key, out string value);
            return value ?? key;
        }

        public static string Get(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }

    public class Version
    {
        public int MinVersion;//服务器最小支持的app版本
        public int MaxVersion;//服务器最新版本资源号
    }

    public class VersionConfig
    {
        public List<string> PatchesAddresses;
    }

    public static class PatchConfig
    {
        public static Version version;
        public static VersionConfig versionConfig;
    }

    public static class AssetConfig
    {
        public const string DefaultPackageName = "DefaultPackage";
        public const string RawFilePackageName = "RawFilePackage";
        public static EPlayMode PlayMode = EPlayMode.OfflinePlayMode;

        public static readonly List<string> BPackageList = new()
        {
            DefaultPackageName,
            RawFilePackageName
        };

        public const int DownloadingMaxNum = 10;
        public const int FailedTryAgain = 3;
    }
}
