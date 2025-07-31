using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public class CoroutineHandler : CustomYieldInstruction
    {
        public Action<CoroutineHandler> remove;
        public CoroutineCompletedHandler completed = new CoroutineCompletedHandler();
        public ICoroutine Owner
        {
            get; private set;
        }
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

        public override bool keepWaiting => Running;

        public CoroutineHandler()
        {
        }
        public CoroutineHandler(ICoroutine owner, IEnumerator coroutine, Action<CoroutineHandler> remove)
        {
            Owner = owner;
            Coroutine = coroutine;
            this.remove = remove;
        }

        public void Start()
        {
            if (Running)
            {
                Log.Debug("当前协程未完成");
                return;
            }
            if (Coroutine == null)
            {
                Log.Debug("协程未指定");
                return;
            }
            Running = true;
            Main.CoroutineService.CoroutineHelper.StartCoroutine(CallWrapper());
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
            completed?.Invoke(Stopped);
            completed.RemoveAllListeners();
            Coroutine = null;
        }
        public CoroutineHandler OnCompleted(UnityAction<bool> action)
        {
            completed.AddListener(action);
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