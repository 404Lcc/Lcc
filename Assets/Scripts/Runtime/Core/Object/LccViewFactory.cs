using System;
using UnityEngine;

namespace Model
{
    public static class LccViewFactory
    {
        public static ObjectBase CreateView(Type type, GameObject gameObject)
        {
            ObjectBase objectBase = (ObjectBase)Activator.CreateInstance(type);
            objectBase.InitObjectBase(gameObject);
            return objectBase;
        }
        public static T CreateView<T>(GameObject gameObject) where T : ObjectBase
        {
            ObjectBase objectBase = Activator.CreateInstance<T>();
            objectBase.InitObjectBase(gameObject);
            return (T)objectBase;
        }
        public static T GetView<T>(GameObject gameObject) where T : ObjectBase
        {
            LccView lccView = GameUtil.GetComponent<LccView>(gameObject);
            return lccView.GetType<T>();
        }
    }
}