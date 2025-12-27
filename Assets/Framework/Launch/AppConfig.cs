using System.Collections.Generic;
using YooAsset;

namespace LccModel
{
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
        public int MinVersion;
        public int MaxVersion;
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