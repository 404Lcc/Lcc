using Model;
using System;
using UnityEngine;

namespace Hotfix
{
    public static class LccViewFactory
    {
        public static AObjectBase CreateView(Type type, GameObject gameObject, object data = null)
        {
            AObjectBase aObjectBase = (AObjectBase)Activator.CreateInstance(type);
            aObjectBase.InitObject(gameObject, data);
            return aObjectBase;
        }
        public static T CreateView<T>(GameObject gameObject, object data = null) where T : AObjectBase
        {
            AObjectBase aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.InitObject(gameObject, data);
            return (T)aObjectBase;
        }
        public static T GetView<T>(GameObject gameObject) where T : AObjectBase
        {
            LccView lccView = gameObject.GetChildComponent<LccView>(typeof(T).FullName);
            return lccView.GetType<T>();
        }
    }
}