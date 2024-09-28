using System;
using System.Threading;
using UnityEngine;

namespace LccHotfix
{
    internal class MainThreadSynchronizationContext : Module
    {
        public static MainThreadSynchronizationContext Instance { get; } = Entry.GetModule<MainThreadSynchronizationContext>();

        private SynchronizationContext last;
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new ThreadSynchronizationContext();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            this.threadSynchronizationContext.Update();
        }

        internal override void Shutdown()
        {
            SynchronizationContext.SetSynchronizationContext(this.last);
        }

        public MainThreadSynchronizationContext()
        {

            last = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
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