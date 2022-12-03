using UnityEngine.Pool;

namespace LccModel
{
    public static class Pool<T> where T : class, new()
    {
        private static ObjectPool<T> _objectPool;

        public static int Count
        {
            get
            {
                return _objectPool.CountAll;
            }
        }


        static Pool()
        {
            _objectPool = new ObjectPool<T>(Create);
        }
        public static T Get()
        {
            return _objectPool.Get();
        }
        public static void Release(T item)
        {
            _objectPool.Release(item);
        }

        private static T Create()
        {
            return new T();
        }
    }
}