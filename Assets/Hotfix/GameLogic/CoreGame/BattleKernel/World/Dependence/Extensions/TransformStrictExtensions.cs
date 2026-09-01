using UnityEngine;

namespace LccHotfix
{
    public static class TransformStrictExtensions
    {
        public static T GetComponentInChildSafe<T>(this Transform root, string path) where T : Component
        {
            var comp = ClientTools.GetChildComponent<T>(root?.gameObject, path);
            if (comp == null)
            {
                KLogger.LogError($"找不到节点或组件 '{path}' under '{root?.name}', 需要 {typeof(T).Name}");
            }

            return comp;
        }

        public static T GetComponentStrict<T>(this Component root) where T : Component
        {
            var comp = root.GetComponent<T>();
            if (comp == null)
            {
                KLogger.LogError($"节点 '{root?.name}' 缺少组件 {typeof(T).Name}");
            }

            return comp;
        }

        public static T GetComponentInChildrenStrict<T>(this Component root, bool includeInactive = false) where T : Component
        {
            var comp = root.GetComponentInChildren<T>(includeInactive);
            if (comp == null)
            {
                KLogger.LogError($"节点 '{root?.name}' 及其子节点缺少组件 {typeof(T).Name}");
            }

            return comp;
        }

        public static Transform FindChildStrict(this Transform root, string path)
        {
            var node = ClientTools.GetChildTransform(root?.gameObject, path);
            if (node == null)
            {
                KLogger.LogError($"找不到节点 '{path}' under '{root?.name}'");
            }

            return node;
        }

        public static GameObject FindChildGameObjectStrict(this Transform root, string path)
        {
            var go = ClientTools.GetChild(root?.gameObject, path);
            if (go == null)
            {
                KLogger.LogError($"找不到节点 '{path}' under '{root?.name}'");
            }

            return go;
        }

        public static RectTransform FindChildRectStrict(this Transform root, string path)
        {
            var node = ClientTools.GetChildTransform(root?.gameObject, path);
            if (node == null)
            {
                KLogger.LogError($"找不到节点 '{path}' under '{root?.name}'");
                return null;
            }

            var rect = node as RectTransform;
            if (rect == null)
            {
                KLogger.LogError($"节点 '{path}' (under '{root.name}') 不是 RectTransform");
            }

            return rect;
        }
    }
}