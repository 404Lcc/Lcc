using System;
using System.Collections;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace LccModel
{
    public class CoroutineHandler
    {
        public Action<CoroutineHandler> remove;
        public CoroutineCompletedHandler OnCompleted = new CoroutineCompletedHandler();
        public IEnumerator Coroutine
        {
            get; private set;
        }
        public bool Paused
        {
            get; private set;
        }
        public bool Running
        {
            get; private set;
        }
        public bool Stopped
        {
            get; private set;
        }
        public CoroutineHandler()
        {
        }
        public CoroutineHandler(IEnumerator coroutine, Action<CoroutineHandler> remove)
        {
            Coroutine = coroutine;
            this.remove = remove;
        }

        public void Start()
        {
            if (Running)
            {
                LogUtil.Log("当前协程未完成");
                return;
            }
            if (Coroutine == null)
            {
                LogUtil.Log("协程未指定");
                return;
            }
            Running = true;
            Object.FindObjectOfType<Init>().StartCoroutine(CallWrapper());
        }
        public void Stop()
        {
            Stopped = true;
            Running = false;
            Finish();
        }
        public void Pause()
        {
            Paused = true;
        }
        public void Resume()
        {
            Paused = false;
        }
        private void Finish()
        {
            remove?.Invoke(this);
            OnCompleted?.Invoke(Stopped);
            OnCompleted.RemoveAllListeners();
            Coroutine = null;
        }
        public CoroutineHandler OnComplete(UnityAction<bool> action)
        {
            OnCompleted.AddListener(action);
            return this;
        }
        private IEnumerator CallWrapper()
        {
            yield return null;
            IEnumerator e = Coroutine;
            while (Running)
            {
                if (Paused)
                {
                    yield return null;
                }
                else
                {
                    if (e != null && e.MoveNext())
                    {
                        yield return e.Current;
                    }
                    else
                    {
                        Running = false;
                    }
                }
            }
            Finish();
        }
    }
}