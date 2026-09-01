using System;

namespace LccHotfix
{
    public class SubobjectLifecycle
    {
        private event Action<EvtSubObjDestroy> _onDestroy;

        public void AddDestroyHandler(Action<EvtSubObjDestroy> handler)
        {
            _onDestroy += handler;
        }

        public void RemoveDestroyHandler(Action<EvtSubObjDestroy> handler)
        {
            _onDestroy -= handler;
        }

        public void DispatchDestroy(EvtSubObjDestroy evt)
        {
            _onDestroy?.Invoke(evt);
        }
    }
}
