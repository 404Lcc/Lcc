using System.IO;

namespace Hotfix
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
            Stream stream;
            FileInfo info = new FileInfo(path);
            if (info.Exists)
            {
                info.Delete();
                stream = info.Create();
            }
            else
            {
                stream = info.Create();
            }
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
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
            Stream stream;
            FileInfo info = new FileInfo(path);
            if (info.Exists)
            {
                stream = info.OpenRead();
            }
            else
            {
                return null;
            }
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
            return bytes;
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