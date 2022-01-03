using UnityEngine;

namespace LccModel
{
    public static class PathUtil
    {
//        public static string GetPath(PathType type, params string[] folders)
//        {
//            string path = string.Empty;
//            string subPath = string.Empty;
//            switch (type)
//            {
//                case PathType.DataPath:
//                    path = Application.dataPath;
//                    break;
//                case PathType.StreamingAssetsPath:
//                    path = Application.streamingAssetsPath;
//                    break;
//                case PathType.PersistentDataPath:
//                    path = Application.persistentDataPath;
//                    break;
//            }
//            //不是编辑器模式 只能在外部空间创建文件夹
//            if (type == PathType.PersistentDataPath && folders.Length == 1 && folders[0].Contains("/"))
//            {
//                return GetPath(path, folders[0]);
//            }
//#if UNITY_EDITOR
//            //编辑器模式下也可以直接创建
//            if (folders.Length == 1 && folders[0].Contains("/"))
//            {
//                if (type == PathType.StreamingAssetsPath)
//                {
//#if UNITY_ANDROID && !UNITY_EDITOR
//                    return GetPath(path, folders[0]);
//#elif UNITY_IPHONE && !UNITY_EDITOR
//                    return "file://" + GetPath(path, folders[0]);
//#elif UNITY_STANDLONE_WIN || UNITY_EDITOR
//                    return "file://" + GetPath(path, folders[0]);
//#endif
//                }
//                return GetPath(path, folders[0]);
//            }
//#endif
//            for (int i = 0; i < folders.Length; i++)
//            {
//                if (i == folders.Length - 1)
//                {
//                    subPath = $"{subPath}{folders[i]}";
//                }
//                else
//                {
//                    subPath = $"{subPath}{folders[i]}/";
//                }
//#if UNITY_EDITOR
//                //编辑器模式下也可以直接创建
//                if (!string.IsNullOrEmpty(subPath))
//                {
//                    DirectoryUtil.CreateDirectory($"{path}/{subPath}");
//                }
//#else
//                //不是编辑器模式 只能在外部空间创建文件夹
//                if (type == PathType.PersistentDataPath && !string.IsNullOrEmpty(subPath))
//                {
//                    DirectoryUtil.CreateDirectory($"{path}/{subPath}");
//                }
//#endif
//            }
//            return $"{path}/{subPath}";
//        }
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
            if (folders.Length == 1 && folders[0].Contains("/"))
            {
                return GetPath(path, folders[0]);
            }
            else
            {
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
            }
            return $"{path}/{subPath}";
        }
        public static string GetPersistentDataPath(params string[] folders)
        {
            string path = Application.persistentDataPath;
            string subPath = string.Empty;
            if (folders.Length == 1 && folders[0].Contains("/"))
            {
                return GetPath(path, folders[0]);
            }
            else
            {
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
            }
            return $"{path}/{subPath}";
        }
        public static string GetStreamingAssetsPath(params string[] folders)
        {
            string path = Application.persistentDataPath;
            string subPath = string.Empty;
            if (folders.Length == 1 && folders[0].Contains("/"))
            {
                return GetPath(path, folders[0]);
            }
            else
            {
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
        private static string GetPath(string path, string folder)
        {
            string[] folders = folder.Split('/');
            folder = folders[0];
            DirectoryUtil.CreateDirectory($"{path}/{folder}");
            string subPath = string.Empty;
            for (int i = 1; i < folders.Length; i++)
            {
                if (string.IsNullOrEmpty(folders[i])) continue;
                subPath = $"{subPath}{folders[i]}";
                DirectoryUtil.CreateDirectory($"{path}/{folder}/{subPath}");
                if (i == folders.Length - 1) continue;
                subPath = $"{subPath}/";
            }
            return $"{path}/{folder}/{subPath}";
        }
    }
}