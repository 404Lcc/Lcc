using System;
using System.Collections.Generic;

namespace LccModel
{
    public class EventComponent : Component
    {
        private readonly Dictionary<Type, List<object>> actionDict = new Dictionary<Type, List<object>>();
        private readonly Dictionary<string, List<object>> fireActionDict = new Dictionary<string, List<object>>();


        public new T Publish<T>(T TEvent) where T : class
        {
            if (actionDict.TryGetValue(typeof(T), out var list))
            {
                foreach (Action<T> action in list)
                {
                    action.Invoke(TEvent);
                }
            }
            return TEvent;
        }

        public new void Subscribe<T>(Action<T> action) where T : class
        {
            var type = typeof(T);
            if (!actionDict.TryGetValue(type, out var list))
            {
                list = new List<object>();
                actionDict.Add(type, list);
            }
            list.Add(action);
        }

        public new void UnSubscribe<T>(Action<T> action) where T : class
        {
            if (actionDict.TryGetValue(typeof(T), out var list))
            {
                list.Remove(action);
            }
        }

        public void FireEvent<T>(string type, T entity) where T : Entity
        {
            if (fireActionDict.TryGetValue(type, out var list))
            {
                foreach (Action<T> action in list)
                {
                    action.Invoke(entity);
                }
            }
        }

        public void OnEvent<T>(string type, Action<T> action) where T : Entity
        {
            if (!fireActionDict.TryGetValue(type, out var list))
            {
                list = new List<object>();
                fireActionDict.Add(type, list);
            }
            list.Add(action);
        }

        public void OffEvent<T>(string type, Action<T> action) where T : Entity
        {
            if (fireActionDict.TryGetValue(type, out var list))
            {
                list.Remove(action);
            }
        }
    }
}