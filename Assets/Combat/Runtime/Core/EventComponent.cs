using System;
using System.Collections.Generic;

namespace LccModel
{
    public class SubscribeSubject : Entity
    {
        public SubscribeSubject DisposeWith(Entity entity)
        {
            AddChildren(entity);
            return this;
        }
    }

    public sealed class EventComponent : Component
    {
        public override bool DefaultEnable { get; set; } = false;
        private Dictionary<Type, List<object>> TypeEvent2ActionLists = new Dictionary<Type, List<object>>();
        private Dictionary<string, Entity> TypeEvent2EntityLists = new Dictionary<string, Entity>();
        private Dictionary<string, List<object>> FireEvent2ActionLists = new Dictionary<string, List<object>>();
        public static bool DebugLog { get; set; } = false;


        public T Publish<T>(T TEvent) where T : class
        {
            if (TypeEvent2ActionLists.TryGetValue(typeof(T), out var actionList))
            {
                var tempList = actionList.ToArray();
                foreach (Action<T> action in tempList)
                {
                    action.Invoke(TEvent);
                }
            }
            return TEvent;
        }

        public SubscribeSubject Subscribe<T>(Action<T> action) where T : class
        {
            var type = typeof(T);
            if (!TypeEvent2ActionLists.TryGetValue(type, out var actionList))
            {
                actionList = new List<object>();
                TypeEvent2ActionLists.Add(type, actionList);
            }
            actionList.Add(action);
            SubscribeSubject subscribeSubject = Parent.AddChildren<SubscribeSubject>(action);
            TypeEvent2EntityLists.Add(subscribeSubject.GetHashCode().ToString(), subscribeSubject);
            return subscribeSubject;
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            if (TypeEvent2ActionLists.TryGetValue(typeof(T), out var actionList))
            {
                actionList.Remove(action);
            }
            if (TypeEvent2EntityLists.TryGetValue(action.GetHashCode().ToString(), out var entity))
            {
                entity.Dispose();
            }
        }

        public void FireEvent<T>(string eventType, T entity) where T : Entity
        {
            if (FireEvent2ActionLists.TryGetValue(eventType, out var actionList))
            {
                var tempList = actionList.ToArray();
                foreach (Action<T> action in tempList)
                {
                    action.Invoke(entity);
                }
            }
        }

        public void OnEvent<T>(string eventType, Action<T> action) where T : Entity
        {
            if (FireEvent2ActionLists.TryGetValue(eventType, out var actionList))
            {
            }
            else
            {
                actionList = new List<object>();
                FireEvent2ActionLists.Add(eventType, actionList);
            }
            actionList.Add(action);
        }

        public void OffEvent<T>(string eventType, Action<T> action) where T : Entity
        {
            if (FireEvent2ActionLists.TryGetValue(eventType, out var actionList))
            {
                actionList.Remove(action);
            }
        }
    }
}