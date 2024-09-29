using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace LccHotfix
{
    public enum SchedulerType
    {
        Main,
        Thread,
        ThreadPool,
    }

    internal class FiberManager : Module
    {
        public static FiberManager Instance { get; } = Entry.GetModule<FiberManager>();

        private readonly IScheduler[] schedulers = new IScheduler[3];

        private int idGenerator = 10000000; // 10000000以下为保留的用于StartSceneConfig的fiber id, 1个区配置1000个纤程，可以配置10000个区
        private ConcurrentDictionary<int, Fiber> fibers = new();

        private MainThreadScheduler mainThreadScheduler;

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            this.mainThreadScheduler.Update();

            this.mainThreadScheduler.LateUpdate();
        }

        internal override void Shutdown()
        {
            foreach (IScheduler scheduler in this.schedulers)
            {
                scheduler.Dispose();
            }

            foreach (var kv in this.fibers)
            {
                kv.Value.Dispose();
            }

            this.fibers = null;
        }

        public bool IsDisposed()
        {
            return fibers == null;
        }

        public FiberManager()
        {
            this.mainThreadScheduler = new MainThreadScheduler(this);
            this.schedulers[(int)SchedulerType.Main] = this.mainThreadScheduler;

#if (ENABLE_VIEW && UNITY_EDITOR) || UNITY_WEBGL
            this.schedulers[(int)SchedulerType.Thread] = this.mainThreadScheduler;
            this.schedulers[(int)SchedulerType.ThreadPool] = this.mainThreadScheduler;
#else
            this.schedulers[(int)SchedulerType.Thread] = new ThreadScheduler(this);
            this.schedulers[(int)SchedulerType.ThreadPool] = new ThreadPoolScheduler(this);
#endif
        }

        public int Create(SchedulerType schedulerType, int fiberId, Action<Fiber> action)
        {
            try
            {
                Fiber fiber = new Fiber(fiberId);

                if (!this.fibers.TryAdd(fiberId, fiber))
                {
                    throw new Exception($"same fiber already existed, if you remove, please await Remove then Create fiber! {fiberId}");
                }
                this.schedulers[(int)schedulerType].Add(fiberId);

                fiber.ThreadSynchronizationContext.Post(() =>
                {
                    Action();
                });

                return fiberId;

                void Action()
                {
                    try
                    {
                        //必须在Fiber线程中执行
                        action.Invoke(fiber);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"init fiber fail: {e}");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"create fiber error: {fiberId}", e);
            }
        }

        public int Create(SchedulerType schedulerType, Action<Fiber> action)
        {
            int fiberId = Interlocked.Increment(ref this.idGenerator);
            return this.Create(schedulerType, fiberId, action);
        }

        public void Remove(int id)
        {
            Fiber fiber = this.Get(id);
            // 要扔到fiber线程执行，否则会出现线程竞争
            fiber.ThreadSynchronizationContext.Post(() =>
            {
                if (this.fibers.Remove(id, out Fiber f))
                {
                    f.Dispose();
                }
            });
        }

        // 不允许外部调用，容易出现多线程问题, 只能通过消息通信，不允许直接获取其它Fiber引用
        internal Fiber Get(int id)
        {
            this.fibers.TryGetValue(id, out Fiber fiber);
            return fiber;
        }

        public int Count()
        {
            return this.fibers.Count;
        }
    }
}