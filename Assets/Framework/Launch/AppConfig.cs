using System.Collections.Generic;
using YooAsset;

namespace LccModel
{
    public static class AppConfig
    {
        public static string AppVersion = "0.1";
        public static string LocalPackageVersion = "0";
        public static string GetVersionStr()
        {
            return $"{AppVersion}.{LocalPackageVersion}";
        }
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

    public static class PatchConfig
    {
        public static bool IsEnablePatcher = false;
    }

    public static class AssetConfig
    {
        public const string DefaultPackageName = "DefaultPackage";
        public const string RawFilePackageName = "RawFilePackage";
        public static EPlayMode PlayMode = EPlayMode.OfflinePlayMode;

        public static readonly List<string> BPackageList = new ()
        {
            DefaultPackageName,
            RawFilePackageName
        };
        
        public const int DownloadingMaxNum = 10;
        public const int FailedTryAgain = 3;
    }
}