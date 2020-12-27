using System.Security.Cryptography;
using System.Text;

namespace LccHotfix
{
    public static class MD5Util
    {
        /// <summary>
        /// 计算MD5
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ComputeMD5(byte[] bytes)
        {
            if (bytes != null)
            {
                //加密结果"x2"结果为32位 "x3"结果为48位 "x4"结果为64位
                MD5 md5 = new MD5CryptoServiceProvider();
                bytes = md5.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return $"{sb}";
            }
            return string.Empty;
        }
        /// <summary>
        /// 计算MD5
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ComputeMD5(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                //加密结果"x2"结果为32位 "x3"结果为48位 "x4"结果为64位
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] bytes = s.GetBytes();
                bytes = md5.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return $"{sb}";
            }
            return string.Empty;
        }
        /// <summary>
        /// 计算文件MD5
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ComputeFileMD5(string path, string name)
        {
            byte[] bytes = FileUtil.GetAsset($"{path}/{name}");
            return ComputeMD5(bytes);
        }
    }
}