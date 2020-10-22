using UnityEngine;

namespace LccModel
{
    public static class GameObjectExpand
    {
        public static GameObject GetChildGameObject(this GameObject gameObject, params string[] childs)
        {
            if (gameObject == null) return null;
            string child = string.Empty;
            for (int i = 0; i < childs.Length - 1; i++)
            {
                child += childs[i] + "/";
            }
            child += childs[childs.Length - 1];
            Transform childTransform = gameObject.transform.Find(child);
            if (childTransform == null) return null;
            return childTransform.gameObject;
        }
        public static void SafeDestroy(this GameObject gameObject)
        {
            if (gameObject == null) return;
            Object.Destroy(gameObject);
        }
        public static T GetChildComponent<T>(this GameObject gameObject, params string[] childs) where T : Component
        {
            GameObject childGameObject = gameObject.GetChildGameObject(childs);
            if (childGameObject == null) return null;
            return childGameObject.GetComponent<T>();
        }
        public static T AddChildComponent<T>(this GameObject gameObject, params string[] childs) where T : Component
        {
            GameObject childGameObject = gameObject.GetChildGameObject(childs);
            if (childGameObject == null) return null;
            return childGameObject.AddComponent<T>();
        }
        public static void SafeDestroy<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null) return;
            T component = gameObject.GetComponent<T>();
            if (component == null) return;
            Object.Destroy(component);
        }
    }
}