using System.Security.Cryptography;
using System.Text;

namespace LccHotfix
{
    public static class MD5Util
    {
        /// <summary>
        /// 生成MD5
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CreateMD5(string path)
        {
            //加密结果"x2"结果为32位 "x3"结果为48位 "x4"结果为64位
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = FileUtil.GetAsset(path);
            if (bytes == null)
            {
                bytes = md5.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return $"{sb}";
            }
            return null;
        }
        /// <summary>
        /// 生成MD5
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CreateMD5(string path, string name)
        {
            return CreateMD5($"{path}/{name}");
        }
    }
}