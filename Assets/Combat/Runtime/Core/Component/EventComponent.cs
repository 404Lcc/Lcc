using System;
using System.Collections.Generic;

namespace LccModel
{
    public class SubscribeSubject : Entity
    {
    }

    public sealed class EventComponent : Component
    {
        public override bool DefaultEnable => false;

        private Dictionary<Type, List<object>> actionDict = new Dictionary<Type, List<object>>();
        private Dictionary<int, Entity> entityDict = new Dictionary<int, Entity>();
        private Dictionary<string, List<object>> fireActionDict = new Dictionary<string, List<object>>();


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

        public new SubscribeSubject Subscribe<T>(Action<T> action) where T : class
        {
            var type = typeof(T);
            if (!actionDict.TryGetValue(type, out var list))
            {
                list = new List<object>();
                actionDict.Add(type, list);
            }
            list.Add(action);
            SubscribeSubject subscribeSubject = Parent.AddChildren<SubscribeSubject>(action);
            entityDict.Add(action.GetHashCode(), subscribeSubject);
            return subscribeSubject;
        }

        public new void UnSubscribe<T>(Action<T> action) where T : class
        {
            if (actionDict.TryGetValue(typeof(T), out var actionList))
            {
                actionList.Remove(action);
            }
            if (entityDict.TryGetValue(action.GetHashCode(), out var entity))
            {
                entity.Dispose();
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