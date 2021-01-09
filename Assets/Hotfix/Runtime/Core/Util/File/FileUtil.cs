using System.Collections.Generic;
using System.IO;

namespace LccHotfix
{
    public static class FileUtil
    {
        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <returns></returns>
        public static bool FileExist(DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Exists) return false;
            return directoryInfo.GetFiles() != null;
        }
        /// <summary>
        /// 获取所有子文件
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="fileInfoList"></param>
        /// <returns></returns>
        public static FileInfo[] GetFiles(DirectoryInfo directoryInfo, List<FileInfo> fileInfoList)
        {
            if (directoryInfo == null) return null;
            if (FileExist(directoryInfo))
            {
                fileInfoList.AddRange(directoryInfo.GetFiles());
            }
            foreach (DirectoryInfo item in directoryInfo.GetDirectories())
            {
                GetFiles(item, fileInfoList);
            }
            return fileInfoList.ToArray().Length == 0 ? null : fileInfoList.ToArray();
        }
        /// <summary>
        /// 保存资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        public static void SaveAsset(string path, byte[] bytes)
        {
            DirectoryUtil.GetDirectoryPath(Path.GetDirectoryName(path));
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                fileStream.Write(bytes, 0, bytes.Length);
            }
        }
        /// <summary>
        /// 保存资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        public static void SaveAsset(string path, string value)
        {
            byte[] bytes = value.GetBytes();
            SaveAsset(path, bytes);
        }
        /// <summary>
        /// 保存资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="bytes"></param>
        public static void SaveAsset(string path, string name, byte[] bytes)
        {
            SaveAsset($"{path}/{name}", bytes);
        }
        /// <summary>
        /// 保存资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SaveAsset(string path, string name, string value)
        {
            SaveAsset($"{path}/{name}", value);
        }
        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] GetAsset(string path)
        {
            byte[] bytes;
            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    bytes = new byte[fileStream.Length];
                    fileStream.Read(bytes, 0, bytes.Length);
                }
                return bytes;
            }
            return null;
        }
        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static byte[] GetAsset(string path, string name)
        {
            return GetAsset($"{path}/{name}");
        }
    }
}