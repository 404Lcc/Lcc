using UnityEngine;

namespace LccHotfix
{
    public static class PathUtil
    {
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
                    path = Application.dataPath;
                    break;
                case PathType.StreamingAssetsPath:
                    path = Application.streamingAssetsPath;
                    break;
                case PathType.PersistentDataPath:
                    path = Application.persistentDataPath;
                    break;
            }
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
            if (string.IsNullOrEmpty(subPath))
            {
                return DirectoryUtil.GetDirectoryPath(path);
            }
            return DirectoryUtil.GetDirectoryPath($"{path}/{subPath}");
        }
    }
}