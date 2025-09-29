using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface IValueEvent
    {
    }

    internal class ValueEventManager : Module, IValueEventService
    {
        private readonly Dictionary<Type, HashSet<Delegate>> _dict = new Dictionary<Type, HashSet<Delegate>>();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {
            _dict.Clear();
        }

        public void AddHandle<T>(Action<T> handle) where T : struct, IValueEvent
        {
            Type eventType = typeof(T);

            if (!_dict.TryGetValue(eventType, out var list))
            {
                list = new HashSet<Delegate>();
                _dict[eventType] = list;
            }

            if (!list.Add(handle))
            {
                Debug.LogError("注册消息重复了");
            }
        }

        public void RemoveHandle<T>(Action<T> handle) where T : struct, IValueEvent
        {
            Type eventType = typeof(T);

            if (_dict.TryGetValue(eventType, out var list))
            {
                list.Remove(handle);

                if (list.Count == 0)
                {
                    _dict.Remove(eventType);
                }
            }
        }

        public void Dispatch<T>(T value) where T : struct, IValueEvent
        {
            Type eventType = typeof(T);

            if (_dict.TryGetValue(eventType, out var list))
            {
                foreach (var item in list)
                {
                    ((Action<T>)item)(value);
                }
            }
        }
    }
}