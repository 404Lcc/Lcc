using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hotfix
{
    public class Singleton<T> : ObjectBase where T : ObjectBase
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
                        _instance.InitObjectBase(gameObject);
                        Object.DontDestroyOnLoad(gameObject);
                    }
                }
                return _instance;
            }
        }
    }
}