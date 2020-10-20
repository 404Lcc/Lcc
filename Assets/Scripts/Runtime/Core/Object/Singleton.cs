using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Model
{
    public class Singleton<T> : AObjectBase where T : AObjectBase
    {
        private static readonly object lockObject = new object();
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockObject)
                    {
                        _instance = Activator.CreateInstance<T>();
                        GameObject gameObject = new GameObject(_instance.ToString());
                        _instance.InitObject(gameObject);
                        Object.DontDestroyOnLoad(gameObject);
                    }
                }
                return _instance;
            }
        }
    }
}