using ET;
using System.Collections.Generic;
using System;

namespace LccModel
{
    public class CoroutineLockQueue
    {
        private int type;
        private long key;

        private CoroutineLock currentCoroutineLock;

        private readonly Queue<WaitCoroutineLock> queue = new Queue<WaitCoroutineLock>();

        public int Count
        {
            get
            {
                return queue.Count;
            }
        }

        public static CoroutineLockQueue Create(int type, long key)
        {
            CoroutineLockQueue coroutineLockQueue = new CoroutineLockQueue();
            coroutineLockQueue.type = type;
            coroutineLockQueue.key = key;
            return coroutineLockQueue;
        }

        public async ETTask<CoroutineLock> Wait(int time)
        {
            if (currentCoroutineLock == null)
            {
                currentCoroutineLock = CoroutineLock.Create(type, key, 1);
                return currentCoroutineLock;
            }
            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
            queue.Enqueue(waitCoroutineLock);
            if (time > 0)
            {
                //long tillTime = Time.Instance.ClientFrameTime() + time;
                Timer.Instance.NewOnceTimer(time, () =>
                {
                    if (waitCoroutineLock.IsDisposed())
                    {
                        return;
                    }
                    waitCoroutineLock.SetException(new Exception("协程超时"));
                });
            }
            currentCoroutineLock = await waitCoroutineLock.Wait();
            return currentCoroutineLock;
        }


        public void Notify(int level)
        {
            //有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (queue.Count > 0)
            {
                WaitCoroutineLock waitCoroutineLock = queue.Dequeue();

                if (waitCoroutineLock.IsDisposed())
                {
                    continue;
                }

                CoroutineLock coroutineLock = CoroutineLock.Create(type, key, level);

                waitCoroutineLock.SetResult(coroutineLock);
                break;
            }
        }
        public void Recycle()
        {
            this.queue.Clear();
            this.key = 0;
            this.type = 0;
            this.currentCoroutineLock = null;
        }
    }
}