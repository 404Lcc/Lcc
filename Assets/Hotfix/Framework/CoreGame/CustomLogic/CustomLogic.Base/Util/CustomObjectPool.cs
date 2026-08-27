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
        // 单类型入池上限默认值；
        public const int DefaultLimit = 1024;
        // Queue 缺省初始容量
        public const int DefaultCapacity = 32;

        private Dictionary<System.Type, Queue<T>> _pool = new();
        
        //Cache(count) 可将实例 _limit 抬高
        private int _limit = DefaultLimit;

        public void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// 预创建指定数量的对象入池；count 大于当前上限时抬高本实例 _limit。
        /// </summary>
        /// <param name="count">预创建数量；≤0 则跳过。</param>
        /// <param name="extraInit">new 之后、入池之前的额外初始化；可为 null。</param>
        public void Cache<CT>(int count = 1, Action<CT> extraInit = null) where CT : T, new()
        {
            if (count <= 0)
                return;

            // 预热数量超过当前上限时抬升，避免随后 Destroy 丢弃多余对象
            if (count > _limit)
                _limit = count;

            System.Type type = typeof(CT);
            if (!_pool.TryGetValue(type, out var queue))
            {
                queue = new Queue<T>(Math.Max(DefaultCapacity, count));
                _pool.Add(type, queue);
            }

            for (int i = 0; i < count; i++)
            {
                var obj = new CT();
                extraInit?.Invoke(obj);
                queue.Enqueue(obj);
            }
        }

        public CT Create<CT>() where CT : class, T, new()
        {
            System.Type type = typeof(CT);
            CT res;
            Queue<T> queue = null;
            if (!_pool.TryGetValue(type, out queue))
            {
                queue = new Queue<T>(DefaultCapacity);
                _pool.Add(type, queue);
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
            if (!_pool.TryGetValue(type, out queue))
            {
                queue = new Queue<T>(DefaultCapacity);
                _pool.Add(type, queue);
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
                CLogger.LogError($"重复的 Destroy type={type}");
                obj.Destroy();
                return;
            }

            obj.Destroy();
            if (!_pool.TryGetValue(type, out var queue))
            {
                queue = new Queue<T>(DefaultCapacity);
                _pool.Add(type, queue);
            }

            if (queue.Count < _limit)
            {
                queue.Enqueue(obj);
            }
        }
    }
}
