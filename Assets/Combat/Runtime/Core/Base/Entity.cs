using System;

namespace LccModel
{
    public class Entity : AObjectBase
    {
        #region ÊÂ¼þ
        public T Publish<T>(T TEvent) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                return TEvent;
            }
            eventComponent.Publish(TEvent);
            return TEvent;
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

        public SubscribeSubject Subscribe<T>(Action<T> action, Entity disposeWith) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                eventComponent = AddComponent<EventComponent>();
            }
            return eventComponent.Subscribe(action).DisposeWith(disposeWith);
        }

        public void UnSubscribe<T>(Action<T> action) where T : class
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent != null)
            {
                eventComponent.UnSubscribe(action);
            }
        }

        public void FireEvent(string eventType)
        {
            FireEvent(eventType, this);
        }

        public void FireEvent(string eventType, Entity entity)
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent != null)
            {
                eventComponent.FireEvent(eventType, entity);
            }
        }

        public void OnEvent(string eventType, Action<Entity> action)
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent == null)
            {
                eventComponent = AddComponent<EventComponent>();
            }
            eventComponent.OnEvent(eventType, action);
        }

        public void OffEvent(string eventType, Action<Entity> action)
        {
            var eventComponent = GetComponent<EventComponent>();
            if (eventComponent != null)
            {
                eventComponent.OffEvent(eventType, action);
            }
        }
        #endregion

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            if (Components.TryGetValue(typeof(T), out var c))
            {
                component = c as T;
                return true;
            }
            component = null;
            return false;
        }
    }
}