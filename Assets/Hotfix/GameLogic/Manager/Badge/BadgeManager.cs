using System.Collections.Generic;

namespace LccHotfix
{
    internal class BadgeManager : Module, IBadgeService
    {
        private HashSet<BadgeHandler> _handlers = new HashSet<BadgeHandler>();
        private List<BadgeHandler> _addCacheList = new List<BadgeHandler>();
        private List<BadgeHandler> _removeCacheList = new List<BadgeHandler>();
        private bool _ticking;

        public void RegisterHandler(BadgeHandler handler)
        {
            if (_ticking)
            {
                _addCacheList.Add(handler);
                return;
            }

            _handlers.Add(handler);
        }

        public void UnRegisterHandler(BadgeHandler handler)
        {
            if (_ticking)
            {
                _removeCacheList.Add(handler);
                return;
            }

            _handlers.Remove(handler);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void LateUpdate()
        {
            base.LateUpdate();

            _ticking = true;

            foreach (var handler in _handlers)
            {
                handler.LateUpdate();
            }

            if (_addCacheList.Count > 0)
            {
                for (int i = 0; i < _addCacheList.Count; ++i)
                {
                    _handlers.Add(_addCacheList[i]);
                }

                _addCacheList.Clear();
            }

            if (_removeCacheList.Count > 0)
            {
                for (int i = 0; i < _removeCacheList.Count; ++i)
                {
                    _handlers.Remove(_removeCacheList[i]);
                }

                _removeCacheList.Clear();
            }

            _ticking = false;
        }

        internal override void Shutdown()
        {
            _handlers.Clear();
        }
    }
}