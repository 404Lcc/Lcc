using System;

namespace LccModel
{
    public class CoroutineLock : IDisposable
    {
        private int type;
        private long key;
        private int level;
        public static CoroutineLock Create(int type, long k, int count)
        {
            CoroutineLock coroutineLock = new CoroutineLock();
            coroutineLock.type = type;
            coroutineLock.key = k;
            coroutineLock.level = count;
            return coroutineLock;
        }
        public void Dispose()
        {
            CoroutineLockManager.Instance.RunNextCoroutine(type, key, level + 1);

            type = CoroutineLockType.None;
            key = 0;
            level = 0;
        }
    }
}