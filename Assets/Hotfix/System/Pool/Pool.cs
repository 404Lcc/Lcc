using System.Collections.Generic;

namespace Hotfix
{
    public class Pool<T>
    {
        public Queue<T> pool;
        public Pool()
        {
        }
        public Pool(int max)
        {
            pool = new Queue<T>(max);
        }
        public int Count
        {
            get
            {
                return pool.Count;
            }
        }
        public void Enqueue(T t)
        {
            pool.Enqueue(t);
        }
        public T Dequeue()
        {
            return pool.Dequeue();
        }
    }
}