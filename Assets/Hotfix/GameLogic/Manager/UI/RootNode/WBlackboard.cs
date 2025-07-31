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

        private Dictionary<string, object> _data = new Dictionary<string, object>();
        private Dictionary<string, List<System.Action<Type, object>>> _observers = new Dictionary<string, List<System.Action<Type, object>>>();
        private bool _isNotifiyng = false;
        private Dictionary<string, List<System.Action<Type, object>>> _addObservers = new Dictionary<string, List<System.Action<Type, object>>>();
        private Dictionary<string, List<System.Action<Type, object>>> _removeObservers = new Dictionary<string, List<System.Action<Type, object>>>();
        private List<Notification> _notifications = new List<Notification>();
        private List<Notification> _notificationsDispatch = new List<Notification>();


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
            if (!this._data.ContainsKey(key))
            {
                this._data[key] = value;
                this._notifications.Add(new Notification(key, Type.ADD, value));
            }
            else
            {
                if ((this._data[key] == null && value != null) || (this._data[key] != null && !this._data[key].Equals(value)))
                {
                    this._data[key] = value;
                    this._notifications.Add(new Notification(key, Type.CHANGE, value));
                }
            }
        }

        public void Unset(string key)
        {
            if (this._data.ContainsKey(key))
            {
                this._data.Remove(key);
                this._notifications.Add(new Notification(key, Type.REMOVE, null));
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
            if (!this._data.ContainsKey(key))
            {
                this._data[key] = value;
                this._notifications.Add(new Notification(key, Type.ADD, value));
                this._notifications.Add(new Notification(key, Type.CHANGE, value));
            }
            else
            {
                int num = (int)this._data[key];
                num += value;
                this._data[key] = num;
                this._notifications.Add(new Notification(key, Type.CHANGE, num));
            }
        }

        public void UnsetNum(string key, int value)
        {
            if (this._data.ContainsKey(key))
            {
                int num = (int)this._data[key];
                num -= value;
                if (num == 0)
                {
                    this._data.Remove(key);
                    this._notifications.Add(new Notification(key, Type.REMOVE, null));
                }
                else
                {
                    this._data[key] = num;
                }
                this._notifications.Add(new Notification(key, Type.CHANGE, num));
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
            if (this._data.ContainsKey(key))
            {
                return _data[key];
            }
            else
            {
                return null;
            }
        }

        public bool Isset(string key)
        {
            return this._data.ContainsKey(key);
        }

        public void AddObserver(string key, System.Action<Type, object> observer)
        {
            List<System.Action<Type, object>> observers = GetObserverList(this._observers, key);
            if (!_isNotifiyng)
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
                    List<System.Action<Type, object>> addObservers = GetObserverList(this._addObservers, key);
                    if (!addObservers.Contains(observer))
                    {
                        addObservers.Add(observer);
                    }
                }

                List<System.Action<Type, object>> removeObservers = GetObserverList(this._removeObservers, key);
                if (removeObservers.Contains(observer))
                {
                    removeObservers.Remove(observer);
                }
            }
        }

        public void RemoveObserver(string key, System.Action<Type, object> observer)
        {
            List<System.Action<Type, object>> observers = GetObserverList(this._observers, key);
            if (!_isNotifiyng)
            {
                if (observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
            else
            {
                List<System.Action<Type, object>> removeObservers = GetObserverList(this._removeObservers, key);
                if (!removeObservers.Contains(observer))
                {
                    if (observers.Contains(observer))
                    {
                        removeObservers.Add(observer);
                    }
                }

                List<System.Action<Type, object>> addObservers = GetObserverList(this._addObservers, key);
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
                return new List<string>(_data.Keys);
            }
        }

        public int NumObservers
        {
            get
            {
                int count = 0;
                foreach (string key in _observers.Keys)
                {
                    count += _observers[key].Count;
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
            if (_notifications.Count == 0)
            {
                return;
            }

            _notificationsDispatch.Clear();
            _notificationsDispatch.AddRange(_notifications);
            _notifications.Clear();

            _isNotifiyng = true;
            foreach (Notification notification in _notificationsDispatch)
            {
                if (!this._observers.ContainsKey(notification.key))
                {
                    //Debug.Log("1 do not notify for key:" + notification.key + " value: " + notification.value);
                    continue;
                }

                List<System.Action<Type, object>> observers = GetObserverList(this._observers, notification.key);
                foreach (System.Action<Type, object> observer in observers)
                {
                    if (this._removeObservers.ContainsKey(notification.key) && this._removeObservers[notification.key].Contains(observer))
                    {
                        continue;
                    }
                    observer(notification.type, notification.value);
                }
            }

            foreach (string key in this._addObservers.Keys)
            {
                GetObserverList(this._observers, key).AddRange(this._addObservers[key]);
            }
            foreach (string key in this._removeObservers.Keys)
            {
                foreach (System.Action<Type, object> action in _removeObservers[key])
                {
                    GetObserverList(this._observers, key).Remove(action);
                }
            }
            this._addObservers.Clear();
            this._removeObservers.Clear();

            _isNotifiyng = false;
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