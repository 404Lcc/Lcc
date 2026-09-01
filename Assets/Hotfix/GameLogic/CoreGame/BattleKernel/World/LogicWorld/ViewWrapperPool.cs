using System;
using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// IViewWrapper 按具体类型分队列的对象池；缺省上限 200，超出不再入池。
    /// 挂在 LogicWorld 上，局结束随 World 释放。Acquire 后须 Bind，DisposeView 后再 Release。
    /// </summary>
    public class ViewWrapperPool
    {
        public const int DefaultMaxUnused = 200;

        // 按运行时 Type 分队列，避免不同 IViewWrapper 实现混池
        private readonly Dictionary<Type, Queue<IViewWrapper>> _pools = new();
        // 未配置的类型走 DefaultMaxUnused
        private readonly Dictionary<Type, int> _maxUnused = new();

        /// <summary>
        /// 指定某具体 View 类型的闲置上限；未调用则使用 DefaultMaxUnused。
        /// </summary>
        /// <param name="type">IViewWrapper 具体类型。</param>
        /// <param name="maxUnused">闲置上限；小于 0 时按 0 处理。</param>
        public void SetMaxUnused(Type type, int maxUnused)
        {
            if (type == null)
            {
                return;
            }

            _maxUnused[type] = maxUnused < 0 ? 0 : maxUnused;
        }

        /// <summary>
        /// 取出指定类型的包装对象；池空则无参 Activator 新建。
        /// </summary>
        /// <param name="type">IViewWrapper 具体类型。</param>
        /// <returns>可 Bind 的实例；type 无效或创建失败返回 null。</returns>
        public IViewWrapper Acquire(Type type)
        {
            if (type == null)
            {
                return null;
            }

            var queue = GetOrCreateQueue(type);
            if (queue.Count > 0)
            {
                return queue.Dequeue();
            }

            return Activator.CreateInstance(type) as IViewWrapper;
        }

        /// <summary>
        /// 归还已 DisposeView 的包装对象；达到上限则丢弃等 GC。
        /// </summary>
        /// <param name="view">待入池实例；null 忽略。</param>
        public void Release(IViewWrapper view)
        {
            if (view == null)
            {
                return;
            }

            var type = view.GetType();
            var queue = GetOrCreateQueue(type);
            var maxUnused = GetMaxUnused(type);
            if (queue.Count < maxUnused)
            {
                queue.Enqueue(view);
            }
        }

        /// <summary>
        /// 清空全部闲置实例。
        /// </summary>
        public void Clear()
        {
            _pools.Clear();
        }

        private Queue<IViewWrapper> GetOrCreateQueue(Type type)
        {
            if (!_pools.TryGetValue(type, out var queue))
            {
                queue = new Queue<IViewWrapper>();
                _pools.Add(type, queue);
            }

            return queue;
        }

        private int GetMaxUnused(Type type)
        {
            if (_maxUnused.TryGetValue(type, out var maxUnused))
            {
                return maxUnused;
            }

            return DefaultMaxUnused;
        }
    }
}
