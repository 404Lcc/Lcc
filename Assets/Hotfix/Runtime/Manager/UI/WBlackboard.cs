﻿using UnityEngine;
using System.Collections.Generic;

namespace LccHotfix
{
    public class WBlackboard
    {
        public enum Type
        {
            ADD,
            REMOVE,
            CHANGE
        }
        private struct Notification
        {
            public string key;
            public Type type;
            public object value;
            public Notification(string key, Type type, object value)
            {
                this.key = key;
                this.type = type;
                this.value = value;
            }
        }

        private Dictionary<string, object> data = new Dictionary<string, object>();
        private Dictionary<string, List<System.Action<Type, object>>> observers = new Dictionary<string, List<System.Action<Type, object>>>();
        private bool isNotifiyng = false;
        private Dictionary<string, List<System.Action<Type, object>>> addObservers = new Dictionary<string, List<System.Action<Type, object>>>();
        private Dictionary<string, List<System.Action<Type, object>>> removeObservers = new Dictionary<string, List<System.Action<Type, object>>>();
        private List<Notification> notifications = new List<Notification>();
        private List<Notification> notificationsDispatch = new List<Notification>();
       

        public void Enable()
        {
           
        }

        public void Disable()
        {
           
        }

        public object this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }

        public void Set(string key)
        {
            if (!Isset(key))
            {
                Set(key, null);
            }
        }

        public void Set(string key, object value)
        {
            if (!this.data.ContainsKey(key))
            {
                this.data[key] = value;
                this.notifications.Add(new Notification(key, Type.ADD, value));
            }
            else
            {
                if ((this.data[key] == null && value != null) || (this.data[key] != null && !this.data[key].Equals(value)))
                {
                    this.data[key] = value;
                    this.notifications.Add(new Notification(key, Type.CHANGE, value));
                }
            }
        }
        
        public void Unset(string key)
        {
            if (this.data.ContainsKey(key))
            {
                this.data.Remove(key);
                this.notifications.Add(new Notification(key, Type.REMOVE, null));
            }
        }
        /// <summary>
        /// 用于计数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetNum(string key, int value)
        {
            if (value <= 0) return;
            if (!this.data.ContainsKey(key))
            {
                this.data[key] = value;
                this.notifications.Add(new Notification(key, Type.ADD, value));
                this.notifications.Add(new Notification(key, Type.CHANGE, value));
            }
            else
            {
                int num = (int) this.data[key];
                num += value;
                this.data[key] = num;
                this.notifications.Add(new Notification(key, Type.CHANGE, num));
            }
        }
        
        public void UnsetNum(string key, int value)
        {
            if (this.data.ContainsKey(key))
            {
                int num = (int) this.data[key];
                num -= value;
                if (num == 0)
                {
                    this.data.Remove(key);
                    this.notifications.Add(new Notification(key, Type.REMOVE, null));
                }
                else
                {
                    this.data[key] = num;
                }
                this.notifications.Add(new Notification(key, Type.CHANGE, num));
            }
        }

        public T Get<T>(string key)
        {
            object result = Get(key);
            if (result == null)
            {
                return default(T);
            }
            return (T)result;
        }

        public object Get(string key)
        {
            if (this.data.ContainsKey(key))
            {
                return data[key];
            }
            else
            {
                return null;
            }
        }

        public bool Isset(string key)
        {
            return this.data.ContainsKey(key);
        }

        public void AddObserver(string key, System.Action<Type, object> observer)
        {
            List<System.Action<Type, object>> observers = GetObserverList(this.observers, key);
            if (!isNotifiyng)
            {
                if (!observers.Contains(observer))
                {
                    observers.Add(observer);
                }
            }
            else
            {
                if (!observers.Contains(observer))
                {
                    List<System.Action<Type, object>> addObservers = GetObserverList(this.addObservers, key);
                    if (!addObservers.Contains(observer))
                    {
                        addObservers.Add(observer);
                    }
                }

                List<System.Action<Type, object>> removeObservers = GetObserverList(this.removeObservers, key);
                if (removeObservers.Contains(observer))
                {
                    removeObservers.Remove(observer);
                }
            }
        }

        public void RemoveObserver(string key, System.Action<Type, object> observer)
        {
            List<System.Action<Type, object>> observers = GetObserverList(this.observers, key);
            if (!isNotifiyng)
            {
                if (observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
            else
            {
                List<System.Action<Type, object>> removeObservers = GetObserverList(this.removeObservers, key);
                if (!removeObservers.Contains(observer))
                {
                    if (observers.Contains(observer))
                    {
                        removeObservers.Add(observer);
                    }
                }

                List<System.Action<Type, object>> addObservers = GetObserverList(this.addObservers, key);
                if (addObservers.Contains(observer))
                {
                    addObservers.Remove(observer);
                }
            }
        }


#if UNITY_EDITOR
        public List<string> Keys
        {
            get
            {
                return new List<string>(data.Keys);
            }
        }

        public int NumObservers
        {
            get
            {
                int count = 0;
                foreach (string key in observers.Keys)
                {
                    count += observers[key].Count;
                }
                return count;
            }
        }
#endif

        public void Update()
        {
            NotifiyObservers();
        }

        private void NotifiyObservers()
        {
            if (notifications.Count == 0)
            {
                return;
            }

            notificationsDispatch.Clear();
            notificationsDispatch.AddRange(notifications);
            notifications.Clear();

            isNotifiyng = true;
            foreach (Notification notification in notificationsDispatch)
            {
                if (!this.observers.ContainsKey(notification.key))
                {
                    //                Debug.Log("1 do not notify for key:" + notification.key + " value: " + notification.value);
                    continue;
                }

                List<System.Action<Type, object>> observers = GetObserverList(this.observers, notification.key);
                foreach (System.Action<Type, object> observer in observers)
                {
                    if (this.removeObservers.ContainsKey(notification.key) && this.removeObservers[notification.key].Contains(observer))
                    {
                        continue;
                    }
                    observer(notification.type, notification.value);
                }
            }

            foreach (string key in this.addObservers.Keys)
            {
                GetObserverList(this.observers, key).AddRange(this.addObservers[key]);
            }
            foreach (string key in this.removeObservers.Keys)
            {
                foreach (System.Action<Type, object> action in removeObservers[key])
                {
                    GetObserverList(this.observers, key).Remove(action);
                }
            }
            this.addObservers.Clear();
            this.removeObservers.Clear();

            isNotifiyng = false;
        }

        private List<System.Action<Type, object>> GetObserverList(Dictionary<string, List<System.Action<Type, object>>> target, string key)
        {
            List<System.Action<Type, object>> observers;
            if (target.ContainsKey(key))
            {
                observers = target[key];
            }
            else
            {
                observers = new List<System.Action<Type, object>>();
                target[key] = observers;
            }
            return observers;
        }
    }
}
