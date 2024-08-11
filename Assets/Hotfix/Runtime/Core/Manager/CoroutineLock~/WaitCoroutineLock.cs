using ET;
using System;

namespace LccModel
{
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
}