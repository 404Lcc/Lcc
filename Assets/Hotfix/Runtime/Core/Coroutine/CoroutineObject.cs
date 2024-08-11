using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    public class CoroutineObject
    {
        private List<CoroutineHandler> _runList = new List<CoroutineHandler>();
        public CoroutineHandler StartCoroutine(IEnumerator enumerator)
        {
            CoroutineHandler handler = new CoroutineHandler(enumerator, Remove);
            handler.Start();
            _runList.Add(handler);
            return handler;
        }
        private void Remove(CoroutineHandler handler)
        {
            _runList.Remove(handler);
        }
        public void StopCoroutine(CoroutineHandler handler)
        {
            handler.Stop();
            _runList.Remove(handler);
        }
        public void PauseCoroutine(CoroutineHandler handler)
        {
            handler.Pause();
        }
        public void ResumeCoroutine(CoroutineHandler handler)
        {
            handler.Resume();
        }
        public void StopAllCoroutines()
        {
            for (int i = _runList.Count - 1; i >= 0; i--)
            {
                var item = _runList[i];
                item.Stop();
            }
        }
    }
}