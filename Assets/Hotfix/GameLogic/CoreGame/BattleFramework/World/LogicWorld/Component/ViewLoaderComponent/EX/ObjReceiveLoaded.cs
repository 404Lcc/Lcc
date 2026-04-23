namespace LccHotfix
{
    public struct ObjReceiveLoaded : IReceiveLoaded
    {
        private GameObjectHandle _handle;

        public void Init(GameObjectHandle handle)
        {
            _handle = handle;
        }

        public GameObjectHandle GetHandle()
        {
            return _handle;
        }

        public void Dispose()
        {
            if (_handle != null)
            {
                if (string.IsNullOrEmpty(_handle.Location))
                {
                    UnityEngine.Debug.LogError("为什么会有一个_handle.Location为null的存在，是怎么出现的？");
                    return;
                }

                _handle.Release(ref _handle);
            }
        }
    }
}