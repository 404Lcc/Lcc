using System.IO;

namespace Model
{
    public static class DirectoryUtil
    {
        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path"></param>
        public static DirectoryInfo CreateDirectory(string path)
        {
            if (Directory.Exists(path)) return null;
            return Directory.CreateDirectory(path);
        }
        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folders"></param>
        public static void CreateDirectory(string path, params string[] folders)
        {
            CreateDirectory(path);
            string subPath = string.Empty;
            for (int i = 0; i < folders.Length; i++)
            {
                subPath += folders[i];
                CreateDirectory(path + "/" + subPath);
                subPath += "/";
            }
        }
    }
}