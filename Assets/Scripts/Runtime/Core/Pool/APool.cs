using System.Collections.Generic;

namespace Model
{
    public abstract class APool<T>
    {
        public int size;
        public Queue<T> poolQueue;
        public APool(int size)
        {
            this.size = size;
            poolQueue = new Queue<T>(size);
        }
        public int Count
        {
            get
            {
                return poolQueue.Count;
            }
        }
        public abstract void InitPool();
        public abstract void Enqueue(T item);
        public abstract T Dequeue();
    }
}