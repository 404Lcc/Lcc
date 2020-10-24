using System.IO;

namespace LccHotfix
{
    public static class FileUtil
    {
        /// <summary>
        /// 保存资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        public static void SaveAsset(string path, byte[] bytes)
        {
            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                }
            }
        }
        /// <summary>
        /// 保存资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="bytes"></param>
        public static void SaveAsset(string path, string name, byte[] bytes)
        {
            SaveAsset(path + "/" + name, bytes);
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
            return GetAsset(path + "/" + name);
        }
    }
}