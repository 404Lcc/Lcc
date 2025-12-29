using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public interface ICanRecycle
    {
        bool IsInPool { get; }
        void Construct();
        void Destroy();
    }

    public class CLNodesPool<T> where T : class, ICanRecycle
    {
        private Dictionary<System.Type, Queue<T>> m_pool = new();
        private int _limit = 200;

        public void Clear()
        {
            m_pool.Clear();
        }

        public void Cache<CT>(int count = 1) where CT : T, new()
        {
            System.Type type = typeof(CT);

            if (!m_pool.ContainsKey(type))
            {
                m_pool.Add(type, new Queue<T>());
            }

            for (int i = 0; i < count; i++)
            {
                m_pool[type].Enqueue(new CT());
            }
        }

        public CT Create<CT>() where CT : class, T, new()
        {
            System.Type type = typeof(CT);
            CT res;
            Queue<T> queue = null;
            if (!m_pool.TryGetValue(type, out queue))
            {
                queue = new Queue<T>();
                m_pool.Add(type, queue);
            }

            if (queue.Count > 0)
            {
                res = (CT)queue.Dequeue();
                res.Construct();
            }
            else
            {
                res = new CT();
            }

            return res;
        }

        public CT Create<CT>(System.Type type) where CT : class, T, new()
        {
            CT res;
            Queue<T> queue = null;
            if (!m_pool.TryGetValue(type, out queue))
            {
                queue = new Queue<T>();
                m_pool.Add(type, queue);
            }

            if (queue.Count > 0)
            {
                res = (CT)queue.Dequeue();
                res.Construct();
            }
            else
            {
                res = Activator.CreateInstance(type) as CT;
            }

            return res;
        }

        public void Destroy(T obj)
        {
            if (obj == null)
            {
                return;
            }

            System.Type type = obj.GetType();

            if (obj.IsInPool)
            {
                LogWrapper.LogError($"重复的 Destroy type={type}");
                obj.Destroy();
                return;
            }

            obj.Destroy();
            if (!m_pool.TryGetValue(type, out var queue))
            {
                queue = new Queue<T>();
                m_pool.Add(type, queue);
            }

            if (queue.Count < _limit)
            {
                queue.Enqueue(obj);
            }
        }
    }
}