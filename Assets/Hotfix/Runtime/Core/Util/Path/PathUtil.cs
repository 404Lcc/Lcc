using UnityEngine;

namespace LccHotfix
{
    public static class PathUtil
    {
        public static string PlatformForAssetBundle
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.WindowsEditor:
                        return "Windows";
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.OSXEditor:
                        return "OSX";
                    case RuntimePlatform.Android:
                        return "Android";
                    case RuntimePlatform.IPhonePlayer:
                        return "IOS";
                    default:
                        return string.Empty;
                }
            }
        }
        public static string GetDataPath(params string[] folders)
        {
            string path = Application.dataPath;
            string subPath = string.Empty;
            for (int i = 0; i < folders.Length; i++)
            {
                if (i == folders.Length - 1)
                {
                    subPath = $"{subPath}{folders[i]}";
                }
                else
                {
                    subPath = $"{subPath}{folders[i]}/";
                }
                if (!string.IsNullOrEmpty(subPath))
                {
                    DirectoryUtil.CreateDirectory($"{path}/{subPath}");
                }
            }
            return $"{path}/{subPath}";
        }
        public static string GetPersistentDataPath(params string[] folders)
        {
            string path = Application.persistentDataPath;
            string subPath = string.Empty;
            for (int i = 0; i < folders.Length; i++)
            {
                if (i == folders.Length - 1)
                {
                    subPath = $"{subPath}{folders[i]}";
                }
                else
                {
                    subPath = $"{subPath}{folders[i]}/";
                }
                if (!string.IsNullOrEmpty(subPath))
                {
                    DirectoryUtil.CreateDirectory($"{path}/{subPath}");
                }
            }
            return $"{path}/{subPath}";
        }
        public static string GetStreamingAssetsPath(params string[] folders)
        {
            string path = Application.streamingAssetsPath;
            string subPath = string.Empty;
            for (int i = 0; i < folders.Length; i++)
            {
                if (i == folders.Length - 1)
                {
                    subPath = $"{subPath}{folders[i]}";
                }
                else
                {
                    subPath = $"{subPath}{folders[i]}/";
                }
                if (!string.IsNullOrEmpty(subPath))
                {
                    DirectoryUtil.CreateDirectory($"{path}/{subPath}");
                }
            }
            return $"{path}/{subPath}";
        }
        public static string GetStreamingAssetsPathWeb(params string[] folders)
        {
            string path = string.Empty;
#if UNITY_ANDROID && !UNITY_EDITOR
            path = Application.streamingAssetsPath;
#elif UNITY_IPHONE && !UNITY_EDITOR
            path = "file://" + Application.streamingAssetsPath;
#elif UNITY_STANDLONE_WIN || UNITY_EDITOR
            path = "file://" + Application.streamingAssetsPath;
#endif
            string subPath = string.Empty;
            for (int i = 0; i < folders.Length; i++)
            {
                if (i == folders.Length - 1)
                {
                    subPath = $"{subPath}{folders[i]}";
                }
                else
                {
                    subPath = $"{subPath}{folders[i]}/";
                }
            }
            return $"{path}/{subPath}";
        }
    }
}