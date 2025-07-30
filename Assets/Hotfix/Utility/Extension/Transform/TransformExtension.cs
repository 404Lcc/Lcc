using UnityEngine;

namespace LccHotfix
{
    public static class TransformExtension
    {
        public static GameObject GetChildGameObject(this Transform transform, params string[] childs)
        {
            if (transform == null) return null;
            string child = string.Empty;
            for (int i = 0; i < childs.Length - 1; i++)
            {
                child += $"{childs[i]}/";
            }
            child += childs[childs.Length - 1];
            Transform childTransform = transform.Find(child);
            if (childTransform == null) return null;
            return childTransform.gameObject;
        }
        public static void SafeDestroy(this Transform transform)
        {
            if (transform == null) return;
            Object.Destroy(transform.gameObject);
        }
        public static T GetChildComponent<T>(this Transform transform, params string[] childs) where T : UnityEngine.Component
        {
            GameObject childGameObject = transform.GetChildGameObject(childs);
            if (childGameObject == null) return null;
            return childGameObject.GetComponent<T>();
        }
        public static T AddChildComponent<T>(this Transform transform, params string[] childs) where T : UnityEngine.Component
        {
            GameObject childGameObject = transform.GetChildGameObject(childs);
            if (childGameObject == null) return null;
            return childGameObject.AddComponent<T>();
        }
        public static void SafeDestroy<T>(this Transform transform) where T : UnityEngine.Component
        {
            if (transform == null) return;
            T component = transform.GetComponent<T>();
            if (component == null) return;
            Object.Destroy(component);
        }
    }
}