using ET;
using System;
using System.Collections.Generic;

namespace LccModel
{
    public static class CoroutineLockType
    {
        public const int None = 0;
        public const int LoadUI = 1;

        public const int Max = 100; // 这个必须最大
    }




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
            CoroutineLockManager.Instance.RunNextCoroutine(this.type, this.key, this.level + 1);

            this.type = CoroutineLockType.None;
            this.key = 0;
            this.level = 0;
        }
    }



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
                return this.queue.Count;
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
                long tillTime = TimeManager.Instance.ClientFrameTime() + time;
                TimerManager.Instance.OnceTimer(tillTime, () =>
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
    public class WaitCoroutineLock
    {
        private ETTask<CoroutineLock> tcs;

        public static WaitCoroutineLock Create()
        {
            WaitCoroutineLock waitCoroutineLock = new WaitCoroutineLock();
            waitCoroutineLock.tcs = ETTask<CoroutineLock>.Create(true);
            return waitCoroutineLock;
        }



        public void SetResult(CoroutineLock coroutineLock)
        {
            if (this.tcs == null)
            {
                throw new NullReferenceException("SetResult tcs is null");
            }
            var t = this.tcs;
            this.tcs = null;
            t.SetResult(coroutineLock);
        }

        public void SetException(Exception exception)
        {
            if (this.tcs == null)
            {
                throw new NullReferenceException("SetException tcs is null");
            }
            var t = this.tcs;
            this.tcs = null;
            t.SetException(exception);
        }

        public bool IsDisposed()
        {
            return this.tcs == null;
        }

        public async ETTask<CoroutineLock> Wait()
        {
            return await this.tcs;
        }
    }

    public class CoroutineLockQueueType
    {
        private readonly int type;

        private readonly Dictionary<long, CoroutineLockQueue> coroutineLockQueues = new Dictionary<long, CoroutineLockQueue>();

        public CoroutineLockQueueType(int type)
        {
            this.type = type;
        }
        private CoroutineLockQueue Get(long key)
        {
            this.coroutineLockQueues.TryGetValue(key, out CoroutineLockQueue queue);
            return queue;
        }
        private CoroutineLockQueue New(long key)
        {
            CoroutineLockQueue queue = CoroutineLockQueue.Create(this.type, key);
            this.coroutineLockQueues.Add(key, queue);
            return queue;
        }
        private void Remove(long key)
        {
            if (this.coroutineLockQueues.Remove(key, out CoroutineLockQueue queue))
            {
                queue.Recycle();
            }
        }
        public async ETTask<CoroutineLock> Wait(long key, int time)
        {
            CoroutineLockQueue queue = this.Get(key) ?? this.New(key);
            return await queue.Wait(time);
        }
        public void Notify(long key, int level)
        {
            CoroutineLockQueue queue = this.Get(key);
            if (queue == null)
            {
                return;
            }

            if (queue.Count == 0)
            {
                this.Remove(key);
            }

            queue.Notify(level);
        }
    }
    public class CoroutineLockManager : Singleton<CoroutineLockManager>, IUpdate
    {
        private readonly List<CoroutineLockQueueType> list = new List<CoroutineLockQueueType>(CoroutineLockType.Max);
        private readonly Queue<(int, long, int)> nextFrameRun = new Queue<(int, long, int)>();



        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            for (int i = 0; i < CoroutineLockType.Max; ++i)
            {
                CoroutineLockQueueType coroutineLockQueueType = new CoroutineLockQueueType(i);
                list.Add(coroutineLockQueueType);
            }
        }



        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000)
        {
            CoroutineLockQueueType coroutineLockQueueType = list[coroutineLockType];
            return await coroutineLockQueueType.Wait(key, time);
        }
        private void Notify(int coroutineLockType, long key, int level)
        {
            CoroutineLockQueueType coroutineLockQueueType = list[coroutineLockType];
            coroutineLockQueueType.Notify(key, level);
        }


        public void RunNextCoroutine(int coroutineLockType, long key, int level)
        {
            //一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100)
            {
                LogUtil.LogWarning($"too much coroutine level: {coroutineLockType} {key}");
            }

            this.nextFrameRun.Enqueue((coroutineLockType, key, level));
        }




        public override void Update()
        {
            base.Update();

            //循环过程中会有对象继续加入队列
            while (this.nextFrameRun.Count > 0)
            {
                (int coroutineLockType, long key, int count) = this.nextFrameRun.Dequeue();
                this.Notify(coroutineLockType, key, count);
            }

        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            list.Clear();
            nextFrameRun.Clear();
        }
    }
}