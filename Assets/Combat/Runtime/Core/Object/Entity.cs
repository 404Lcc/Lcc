using System;

namespace LccModel
{
    public class Entity : AObjectBase
    {
        #region ÊÂ¼þ
        public T Publish<T>(T t) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                return t;
            }
            eventComponent.Publish(t);
            return t;
        }
        public SubscribeSubject Subscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                eventComponent = AddComponent<EventComponent>();
            }
            return eventComponent.Subscribe(action);
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent != null)
            {
                eventComponent.UnSubscribe(action);
            }
        }

        public void FireEvent(string type)
        {
            FireEvent(type, this);
        }

        public void FireEvent(string type, Entity entity)
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent != null)
            {
                eventComponent.FireEvent(type, entity);
            }
        }

        public void OnEvent(string type, Action<Entity> action)
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                eventComponent = AddComponent<EventComponent>();
            }
            eventComponent.OnEvent(type, action);
        }

        public void OffEvent(string type, Action<Entity> action)
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent != null)
            {
                eventComponent.OffEvent(type, action);
            }
        }
        #endregion

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            if (Components.TryGetValue(typeof(T), out var temp))
            {
                component = temp as T;
                return true;
            }
            component = null;
            return false;
        }
    }
}