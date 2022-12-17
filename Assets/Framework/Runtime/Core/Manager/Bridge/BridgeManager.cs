using System;
using System.Collections.Generic;

namespace LccModel
{
    public class BridgeManager : AObjectBase
    {
        public static BridgeManager Instance { get; set; }
        private Dictionary<string, List<Delegate>> _dict = new Dictionary<string, List<Delegate>>();

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            _dict.Clear();
            Instance = null;
        }
        #region Publish
        public void Publish(string eventName)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Action action)
                    {
                        action();
                    }
                }
            }
        }
        public void Publish<T1>(string eventName, T1 t1)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Action<T1> action)
                    {
                        action(t1);
                    }
                }
            }
        }
        public void Publish<T1, T2>(string eventName, T1 t1, T2 t2)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Action<T1, T2> action)
                    {
                        action(t1, t2);
                    }
                }
            }
        }
        public void Publish<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Action<T1, T2, T3> action)
                    {
                        action(t1, t2, t3);
                    }
                }
            }
        }
        public void Publish<T1, T2, T3, T4>(string eventName, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Action<T1, T2, T3, T4> action)
                    {
                        action(t1, t2, t3, t4);
                    }
                }
            }
        }
        public Result Publish<Result>(string eventName)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Func<Result> func)
                    {
                        return func();
                    }
                }
            }
            return default;
        }
        public Result Publish<T1, Result>(string eventName, T1 t1)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Func<T1, Result> func)
                    {
                        return func(t1);
                    }
                }
            }
            return default;
        }
        public Result Publish<T1, T2, Result>(string eventName, T1 t1, T2 t2)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Func<T1, T2, Result> func)
                    {
                        func(t1, t2);
                    }
                }
            }
            return default;
        }
        public Result Publish<T1, T2, T3, Result>(string eventName, T1 t1, T2 t2, T3 t3)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Func<T1, T2, T3, Result> func)
                    {
                        return func(t1, t2, t3);
                    }
                }
            }
            return default;
        }
        public Result Publish<T1, T2, T3, T4, Result>(string eventName, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                foreach (var item in list)
                {
                    if (item is Func<T1, T2, T3, T4, Result> func)
                    {
                        return func(t1, t2, t3, t4);
                    }
                }
            }
            return default;
        }
        #endregion
        #region Add
        public void Add(string eventName, Action action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(action);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(action);
                _dict.Add(eventName, list);
            }
        }
        public void Add<T1>(string eventName, Action<T1> action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(action);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(action);
                _dict.Add(eventName, list);
            }
        }
        public void Add<T1, T2>(string eventName, Action<T1, T2> action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(action);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(action);
                _dict.Add(eventName, list);
            }
        }
        public void Add<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(action);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(action);
                _dict.Add(eventName, list);
            }
        }
        public void Add<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(action);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(action);
                _dict.Add(eventName, list);
            }
        }
        public void Add<Result>(string eventName, Func<Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(func);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(func);
                _dict.Add(eventName, list);
            }
        }
        public void Add<T1, Result>(string eventName, Func<T1, Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(func);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(func);
                _dict.Add(eventName, list);
            }
        }
        public void Add<T1, T2, Result>(string eventName, Func<T1, T2, Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(func);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(func);
                _dict.Add(eventName, list);
            }
        }
        public void Add<T1, T2, T3, Result>(string eventName, Func<T1, T2, T3, Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(func);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(func);
                _dict.Add(eventName, list);
            }
        }
        public void Add<T1, T2, T3, T4, Result>(string eventName, Func<T1, T2, T3, T4, Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Add(func);
            }
            else
            {
                list = new List<Delegate>();
                list.Add(func);
                _dict.Add(eventName, list);
            }
        }
        #endregion
        #region Remove
        public void Remove(string eventName, Action action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(action);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        public void Remove<T1>(string eventName, Action<T1> action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(action);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        public void Remove<T1, T2>(string eventName, Action<T1, T2> action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(action);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        public void Remove<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(action);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        public void Remove<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> action)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(action);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        public void Remove<Result>(string eventName, Func<Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(func);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        public void Remove<T1, Result>(string eventName, Func<T1, Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(func);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        public void Remove<T1, T2, Result>(string eventName, Func<T1, T2, Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(func);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        public void Remove<T1, T2, T3, Result>(string eventName, Func<T1, T2, T3, Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(func);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        public void Remove<T1, T2, T3, T4, Result>(string eventName, Func<T1, T2, T3, T4, Result> func)
        {
            if (_dict.TryGetValue(eventName, out List<Delegate> list))
            {
                list.Remove(func);
                if (list.Count == 0)
                {
                    _dict.Remove(eventName);
                }
            }
        }
        #endregion
    }
}