using LccModel;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace LccHotfix
{
    public class ThreadSynchronizationContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

        private Action _action;
        public void Update()
        {
            while (true)
            {
                if (!queue.TryDequeue(out _action))
                {
                    return;
                }

                try
                {
                    _action();
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            Post(() => callback(state));
        }

        public void Post(Action action)
        {
            queue.Enqueue(action);
        }
    }
}