using System;

namespace LccHotfix
{
    public class Fiber : IDisposable
    {
        // 该字段只能框架使用，绝对不能改成public，改了后果自负
        [ThreadStatic]
        public static Fiber Instance;

        public bool IsDisposed;

        public int Id;

        public ThreadSynchronizationContext ThreadSynchronizationContext { get; }


        internal Fiber(int id)
        {
            this.Id = id;
            this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
        }

        internal void Update()
        {
        }

        internal void LateUpdate()
        {
            try
            {
                this.ThreadSynchronizationContext.Update();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            this.IsDisposed = true;
        }
    }
}