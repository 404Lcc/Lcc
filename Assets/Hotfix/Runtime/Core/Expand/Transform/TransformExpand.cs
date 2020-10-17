using UnityEngine;

namespace Hotfix
{
    public static class TransformExpand
    {
        public static Transform GetChildTransform(this Transform transform, params string[] childs)
        {
            if (transform == null) return null;
            string child = string.Empty;
            for (int i = 0; i < childs.Length - 1; i++)
            {
                child += childs[i] + "/";
            }
            child += child[childs.Length - 1];
            Transform childTransform = transform.Find(child);
            if (childTransform == null) return null;
            return childTransform;
        }
        public static void SafeDestroy(this Transform transform)
        {
            if (transform == null) return;
            Object.Destroy(transform.gameObject);
        }
        public static T GetChildComponent<T>(this Transform transform, params string[] childs) where T : Component
        {
            Transform childTransform = transform.GetChildTransform(childs);
            if (childTransform == null) return null;
            return childTransform.GetComponent<T>();
        }
        public static T AddChildComponent<T>(this Transform transform, params string[] childs) where T : Component
        {
            Transform childTransform = transform.GetChildTransform(childs);
            if (childTransform == null) return null;
            return childTransform.gameObject.AddComponent<T>();
        }
        public static void SafeDestroy<T>(this Transform transform) where T : Component
        {
            if (transform == null) return;
            T component = transform.GetComponent<T>();
            if (component == null) return;
            Object.Destroy(component);
        }
    }
}