using UnityEngine.Pool;

namespace LccModel
{
    public abstract class APool<T> where T : class
    {
        protected ObjectPool<T> objectPool;
        public int _defaultSize;
        public int _maxSize;
        public APool(int defaultSize, int maxSize)
        {
            _defaultSize = defaultSize;
            _maxSize = maxSize;
            objectPool = new ObjectPool<T>(Create, Get, Release, Destroy, true, _defaultSize, maxSize);
        }
        public int Count
        {
            get
            {
                return objectPool.CountActive;
            }
        }
        protected abstract T Create();
        protected abstract void Get(T item);
        protected abstract void Release(T item);
        protected abstract void Destroy(T item);
        public T GetOnPool()
        {
            return objectPool.Get();
        }
        public void ReleaseOnPool(T item)
        {
            objectPool.Release(item);
        }
        public void ClearOnPool()
        {
            objectPool.Clear();
        }
    }
}