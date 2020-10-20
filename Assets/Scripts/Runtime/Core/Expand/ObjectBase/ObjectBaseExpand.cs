using UnityEngine;

namespace Model
{
    public static class ObjectBaseExpand
    {
        public static GameObject GetChildGameObject(this AObjectBase aObjectBase, params string[] childs)
        {
            if (aObjectBase == null) return null;
            string child = string.Empty;
            for (int i = 0; i < childs.Length - 1; i++)
            {
                child += childs[i] + "/";
            }
            child += childs[childs.Length - 1];
            Transform childTransform = aObjectBase.gameObject.transform.Find(child);
            if (childTransform == null) return null;
            return childTransform.gameObject;
        }
        public static void SafeDestroy(this AObjectBase aObjectBase)
        {
            if (aObjectBase == null) return;
            Object.Destroy(aObjectBase.gameObject);
        }
        public static T GetComponent<T>(this AObjectBase aObjectBase) where T : AObjectBase
        {
            if (aObjectBase == null) return null;
            return LccViewFactory.GetView<T>(aObjectBase.gameObject);
        }
        public static T GetChildComponent<T>(this AObjectBase aObjectBase, params string[] childs) where T : AObjectBase
        {
            GameObject childGameObject = aObjectBase.GetChildGameObject(childs);
            if (childGameObject == null) return null;
            return LccViewFactory.GetView<T>(childGameObject);
        }
        public static T AddChildComponent<T>(this AObjectBase aObjectBase, params string[] childs) where T : AObjectBase
        {
            GameObject childGameObject = aObjectBase.GetChildGameObject(childs);
            if (childGameObject == null) return null;
            return LccViewFactory.CreateView<T>(childGameObject);
        }
        public static void SafeDestroy<T>(this AObjectBase aObjectBase) where T : AObjectBase
        {
            if (aObjectBase == null) return;
            T component = aObjectBase.GetComponent<T>();
            if (component == null) return;
            Object.Destroy(component.gameObject);
        }
    }
}