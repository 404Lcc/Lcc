using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public static class CoroutineExtension
    {
        public static CoroutineHandler StartCoroutine(this ICoroutine owner, IEnumerator coroutine)
        {
            return Main.CoroutineService.StartCoroutine(owner, coroutine);
        }
        public static void StopAllCoroutines(this ICoroutine owner)
        {
            Main.CoroutineService.StopAllCoroutines(owner);
        }
    }
    
    public interface ICoroutine
    {
    }
    
    internal class CoroutineManager : Module, ICoroutineService
    {
        internal override void Shutdown()
        {
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }
        private Dictionary<ICoroutine, List<CoroutineHandler>> _coroutineDict = new Dictionary<ICoroutine, List<CoroutineHandler>>();

        public CoroutineHandler StartCoroutine(ICoroutine owner, IEnumerator coroutine)
        {
            CoroutineHandler handler = new CoroutineHandler(owner, coroutine, Remove);
            handler.Start();
            if (_coroutineDict.TryGetValue(owner, out var list))
            {
                list.Add(handler);
            }
            else
            {
                list = new List<CoroutineHandler>();
                list.Add(handler);
                _coroutineDict.Add(owner, list);
            }

            return handler;
        }

        private void Remove(CoroutineHandler handler)
        {
            var owner = handler.Owner;
            if (_coroutineDict.TryGetValue(owner, out var list))
            {
                list.Remove(handler);
                if (list.Count == 0)
                {
                    _coroutineDict.Remove(owner);
                }
            }
        }

        public void StopCoroutine(CoroutineHandler handler)
        {
            handler.Stop();
        }

        public void PauseCoroutine(CoroutineHandler handler)
        {
            handler.Pause();
        }

        public void ResumeCoroutine(CoroutineHandler handler)
        {
            handler.Resume();
        }

        public void StopAllCoroutines(ICoroutine owner)
        {
            if (_coroutineDict.TryGetValue(owner, out var list))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var item = list[i];
                    item.Stop();
                }
                _coroutineDict.Remove(owner);
            }
        }

        public void StopAllTypeCoroutines()
        {
            var list = _coroutineDict.Values.ToList();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                for (int j = item.Count - 1; j >= 0; j--)
                {
                    var item1 = item[j];
                    item1.Stop();
                }
            }
            _coroutineDict.Clear();
        }
    }
}