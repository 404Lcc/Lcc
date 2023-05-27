using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class EventArgs<T> : IEventArgs
    {
        public T arg;

        public static IEventArgs CreateEventArgs(T value)
        {
            return new EventArgs<T>(value);
        }

        private EventArgs(T arg)
        {
            this.arg = arg;
        }
    }
    public static class EventArgsExtend
    {
        public static T GetValue<T>(this IEventArgs args)
        {
            T result = default;
            if (args is EventArgs<T>)
            {
                result = ((EventArgs<T>)args).arg;
            }
            return result;
        }
    }

    public class Event : Singleton<Event>
    {
        private Dictionary<EventType, List<IEventListener>> _eventDic = new Dictionary<EventType, List<IEventListener>>();

        public void AddListener(EventType eventType, IEventListener listener)
        {
            if (this._eventDic == null)
            {
                return;
            }

            List<IEventListener> list = null;
            this._eventDic.TryGetValue(eventType, out list);
            if (list == null)
            {
                list = new List<IEventListener>();
                this._eventDic[eventType] = list;
            }
            list.Add(listener);
        }

        public void RemoveListener(EventType eventType, IEventListener listener)
        {
            if (this._eventDic == null)
            {
                return;
            }

            List<IEventListener> list = null;
            this._eventDic.TryGetValue(eventType, out list);
            if (list != null && list.Contains(listener))
            {
                list.Remove(listener);
            }
        }

        public void Publish(EventType eventType)
        {
            if (this._eventDic == null)
            {
                return;
            }

            List<IEventListener> list = null;
            this._eventDic.TryGetValue(eventType, out list);
            if (list == null || list.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                IEventListener eventListener = list[i];
                if (eventListener != null)
                {
                    eventListener.HandleEvent(eventType);
                }
            }
        }

        public void Publish<T1>(EventType eventType, T1 args1)
        {
            IEventArgs eventArgs1 = EventArgs<T1>.CreateEventArgs(args1);
            if (this._eventDic == null)
            {
                return;
            }

            List<IEventListener> list = null;
            this._eventDic.TryGetValue(eventType, out list);
            if (list == null || list.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                IEventListener eventListener = list[i];
                if (eventListener != null)
                {
                    eventListener.HandleEvent(eventType, eventArgs1);
                }
            }
        }

        public void Publish<T1, T2>(EventType eventType, T1 args1, T2 args2)
        {
            IEventArgs eventArgs1 = EventArgs<T1>.CreateEventArgs(args1);
            IEventArgs eventArgs2 = EventArgs<T2>.CreateEventArgs(args2);
            if (this._eventDic == null)
            {
                return;
            }

            List<IEventListener> list = null;
            this._eventDic.TryGetValue(eventType, out list);
            if (list == null || list.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                IEventListener eventListener = list[i];
                if (eventListener != null)
                {
                    eventListener.HandleEvent(eventType, eventArgs1, eventArgs2);
                }
            }
        }

        public void Publish<T1, T2, T3>(EventType eventType, T1 args1, T2 args2, T3 args3)
        {
            IEventArgs eventArgs1 = EventArgs<T1>.CreateEventArgs(args1);
            IEventArgs eventArgs2 = EventArgs<T2>.CreateEventArgs(args2);
            IEventArgs eventArgs3 = EventArgs<T3>.CreateEventArgs(args3);

            if (this._eventDic == null)
            {
                return;
            }

            List<IEventListener> list = null;
            this._eventDic.TryGetValue(eventType, out list);
            if (list == null || list.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                IEventListener eventListener = list[i];
                if (eventListener != null)
                {
                    eventListener.HandleEvent(eventType, eventArgs1, eventArgs2, eventArgs3);
                }
            }
        }


        public void Publish<T1, T2, T3, T4>(EventType eventType, T1 args1, T2 args2, T3 args3, T4 args4)
        {
            IEventArgs eventArgs1 = EventArgs<T1>.CreateEventArgs(args1);
            IEventArgs eventArgs2 = EventArgs<T2>.CreateEventArgs(args2);
            IEventArgs eventArgs3 = EventArgs<T3>.CreateEventArgs(args3);
            IEventArgs eventArgs4 = EventArgs<T4>.CreateEventArgs(args4);

            if (this._eventDic == null)
            {
                return;
            }

            List<IEventListener> list = null;
            this._eventDic.TryGetValue(eventType, out list);
            if (list == null || list.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                IEventListener eventListener = list[i];
                if (eventListener != null)
                {
                    eventListener.HandleEvent(eventType, eventArgs1, eventArgs2, eventArgs3, eventArgs4);
                }
            }
        }
    }
}