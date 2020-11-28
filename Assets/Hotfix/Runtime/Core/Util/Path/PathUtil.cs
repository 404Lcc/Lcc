using UnityEngine;

namespace LccHotfix
{
    public static class PathUtil
    {
        public static string GetDataPath
        {
            get
            {
                return $"{Application.dataPath}/";
            }
        }
        public static string GetStreamingAssetsPath
        {
            get
            {
                return $"{Application.streamingAssetsPath}/";
            }
        }
        public static string GetPersistentDataPath
        {
            get
            {
                return $"{Application.persistentDataPath}/";
            }
        }
        /// <summary>
        /// 获取路径
        /// </summary>
        /// <param name="type"></param>
        /// <param name="folders"></param>
        /// <returns></returns>
        public static string GetPath(PathType type, params string[] folders)
        {
            string path = string.Empty;
            string subPath = string.Empty;
            switch (type)
            {
                case PathType.DataPath:
                    path = GetDataPath;
                    break;
                case PathType.StreamingAssetsPath:
                    path = GetStreamingAssetsPath;
                    break;
                case PathType.PersistentDataPath:
                    path = GetPersistentDataPath;
                    break;
            }
            for (int i = 0; i < folders.Length; i++)
            {
                subPath += $"{folders[i]}/";
                DirectoryUtil.CreateDirectory(path + subPath);
            }
            if (string.IsNullOrEmpty(subPath)) return path;
            return path + subPath;
        }
    }
}