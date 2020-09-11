using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hotfix
{
    public class Singleton<T> : ObjectBase where T : ObjectBase
    {
        private static readonly object lockobject = new object();
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockobject)
                    {
                        _instance = Activator.CreateInstance<T>();
                        GameObject obj = new GameObject(_instance.ToString());
                        _instance.InitObjectBase(obj);
                        Object.DontDestroyOnLoad(obj);
                    }
                }
                return _instance;
            }
        }
    }
}