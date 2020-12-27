using System.Text;

namespace LccHotfix
{
    public static class ByteExpand
    {
        public static string GetString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}