using System.Collections.Generic;

namespace Hotfix
{
    public class Pool<T>
    {
        public Queue<T> poolQueue;
        public Pool()
        {
        }
        public Pool(int max)
        {
            poolQueue = new Queue<T>(max);
        }
        public int Count
        {
            get
            {
                return poolQueue.Count;
            }
        }
        public void Enqueue(T t)
        {
            poolQueue.Enqueue(t);
        }
        public T Dequeue()
        {
            return poolQueue.Dequeue();
        }
    }
}