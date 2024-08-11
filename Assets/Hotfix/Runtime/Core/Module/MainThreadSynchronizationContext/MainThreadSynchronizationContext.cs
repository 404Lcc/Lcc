using System;
using System.Threading;
using UnityEngine;

namespace LccHotfix
{
    public class MainThreadSynchronizationContext : Singleton<MainThreadSynchronizationContext>, ISingletonUpdate
    {
        private SynchronizationContext last;
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new ThreadSynchronizationContext();

        public override void Register()
        {
            base.Register();

            last = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
        }

        public override void Destroy()
        {
            base.Destroy();

            SynchronizationContext.SetSynchronizationContext(this.last);
        }

        public void Update()
        {
            this.threadSynchronizationContext.Update();
        }

        public void Post(SendOrPostCallback callback, object state)
        {
            this.Post(() => callback(state));
        }

        public void Post(Action action)
        {
            this.threadSynchronizationContext.Post(action);
        }
    }
}