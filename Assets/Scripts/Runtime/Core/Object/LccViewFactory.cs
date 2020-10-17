using System;
using UnityEngine;

namespace Model
{
    public static class LccViewFactory
    {
        public static ObjectBase CreateView(Type type, GameObject gameObject, object data = null)
        {
            ObjectBase objectBase = (ObjectBase)Activator.CreateInstance(type);
            objectBase.InitObjectBase(gameObject, data);
            return objectBase;
        }
        public static T CreateView<T>(GameObject gameObject, object data = null) where T : ObjectBase
        {
            ObjectBase objectBase = Activator.CreateInstance<T>();
            objectBase.InitObjectBase(gameObject, data);
            return (T)objectBase;
        }
        public static T GetView<T>(GameObject gameObject) where T : ObjectBase
        {
            LccView lccView = gameObject.GetChildComponent<LccView>(typeof(T).FullName);
            return lccView.GetType<T>();
        }
    }
}