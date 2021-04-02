using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public class Singleton<T> : AObjectBase where T : AObjectBase
    {
        private static readonly object _lockObject = new object();
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        _instance = Activator.CreateInstance<T>();
                        GameObject gameObject = new GameObject($"{_instance}");
                        _instance.InitObject(gameObject);
                        Object.DontDestroyOnLoad(gameObject);
                    }
                }
                return _instance;
            }
        }
    }
}