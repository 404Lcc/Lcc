using System.Collections;
using System.Collections.Generic;

namespace LccModel
{
    public class CoroutineManager : Singleton<CoroutineManager>
    {
        private List<CoroutineHandler> _runList = new List<CoroutineHandler>();
        public CoroutineHandler StartCoroutineTask(IEnumerator enumerator)
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
        public void StopAllCoroutineTask()
        {
            foreach (var item in _runList)
            {
                item.Stop();
            }
        }
    }
}