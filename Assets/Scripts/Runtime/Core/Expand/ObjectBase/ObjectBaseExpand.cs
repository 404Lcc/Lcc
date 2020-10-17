using UnityEngine;

namespace Model
{
    public static class ObjectBaseExpand
    {
        public static GameObject GetChildGameObject(this ObjectBase objectBase, params string[] childs)
        {
            if (objectBase == null) return null;
            string child = string.Empty;
            for (int i = 0; i < childs.Length - 1; i++)
            {
                child += childs[i] + "/";
            }
            child += childs[childs.Length - 1];
            Transform childTransform = objectBase.gameObject.transform.Find(child);
            if (childTransform == null) return null;
            return childTransform.gameObject;
        }
        public static void SafeDestroy(this ObjectBase objectBase)
        {
            if (objectBase == null) return;
            Object.Destroy(objectBase.gameObject);
        }
        public static T GetComponent<T>(this ObjectBase objectBase) where T : ObjectBase
        {
            if (objectBase == null) return null;
            return LccViewFactory.GetView<T>(objectBase.gameObject);
        }
        public static T GetChildComponent<T>(this ObjectBase objectBase, params string[] childs) where T : ObjectBase
        {
            GameObject childGameObject = objectBase.GetChildGameObject(childs);
            if (childGameObject == null) return null;
            return LccViewFactory.GetView<T>(childGameObject);
        }
        public static T AddChildComponent<T>(this ObjectBase objectBase, params string[] childs) where T : ObjectBase
        {
            GameObject childGameObject = objectBase.GetChildGameObject(childs);
            if (childGameObject == null) return null;
            return LccViewFactory.CreateView<T>(childGameObject);
        }
        public static void SafeDestroy<T>(this ObjectBase objectBase) where T : ObjectBase
        {
            if (objectBase == null) return;
            T component = objectBase.GetComponent<T>();
            if (component == null) return;
            Object.Destroy(component.gameObject);
        }
    }
}