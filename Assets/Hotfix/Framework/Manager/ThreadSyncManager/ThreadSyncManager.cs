using System;

namespace LccHotfix
{
    public class ThreadSyncManager : Module, IThreadSyncService
    {
        public ThreadSynchronizationContext _context;

        public ThreadSyncManager()
        {
            _context = new ThreadSynchronizationContext();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            _context?.Update();
        }

        internal override void Shutdown()
        {
            _context = null;
        }

        public void Post(Action action)
        {
            _context?.Post(action);
        }
    }
}