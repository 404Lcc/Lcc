using System.Threading;

namespace LccHotfix
{
    public static class IdUtility
    {
        public static int id;
        public static long GenerateId()
        {
            return Interlocked.Increment(ref id);
        }
    }
}