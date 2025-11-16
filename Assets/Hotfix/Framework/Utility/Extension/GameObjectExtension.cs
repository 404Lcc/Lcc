using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 定义了GameObject需要具备的能力
    /// </summary>
    public interface IGameObjectUtility
    {
        public GameObject GetUserWidget();
        public GameObject FindChild(string childName);
        public T FindChildComponent<T>(string childName) where T : Component;
        public GameObject FindDirect(string childPath);
        public T FindDirectComponent<T>(string childPath) where T : Component;
    }

    /// <summary>
    /// GameObject基础实现
    /// </summary>
    public abstract class GameObjectBaseUtility : IGameObjectUtility
    {
        private Dictionary<string, GameObject> _fastPathChild;

        public abstract GameObject GetUserWidget();

        public virtual void ClearBaseUtility()
        {
            _fastPathChild = null;
        }

        public GameObject FindChild(string childName)
        {
            var widgetTransform = GetUserWidget()?.transform;
            if (widgetTransform is null)
            {
                Debug.LogError($"在{this}中无法找到子项[{childName}]，用户控件无效。");
                return null;
            }

            GameObject child;
            bool found = (_fastPathChild ??= new Dictionary<string, GameObject>()).TryGetValue(childName, out child);
            if (!found)
            {
                var childTransforms = widgetTransform.GetComponentsInChildren<Transform>(true);
                foreach (var transform in childTransforms)
                {
                    var gameObject = transform.gameObject;
                    _fastPathChild[gameObject.name] = gameObject;
                }

                found = _fastPathChild.TryGetValue(childName, out child);
                if (!found)
                {
                    _fastPathChild[childName] = null;
                }
            }

            return child;
        }

        public T FindChildComponent<T>(string childName) where T : Component
        {
            return FindChild(childName)?.GetComponent<T>();
        }

        public GameObject FindDirect(string childPath)
        {
            var widgetTransform = GetUserWidget()?.transform;
            if (widgetTransform is null)
            {
                Debug.LogError($"在{this}中无法找到路径[{childPath}]，用户控件无效。");
                return null;
            }

            GameObject child;
            bool found = (_fastPathChild ??= new Dictionary<string, GameObject>()).TryGetValue(childPath, out child);
            if (!found)
            {
                child = widgetTransform.Find(childPath)?.gameObject;
                _fastPathChild[childPath] = child ??= null;
                var pathArray = childPath.Split("/");
                if (0 != (pathArray?.Length ?? 0))
                {
                    _fastPathChild[pathArray.Last()] = child;
                }
            }

            return child;
        }

        public T FindDirectComponent<T>(string childPath) where T : Component
        {
            return FindDirect(childPath)?.GetComponent<T>();
        }
    }

    public static class GameObjectExtension
    {
        public static void SetVisible(this IGameObjectUtility baseUtility, string childName, bool visible)
        {
            baseUtility.FindChild(childName).SetVisible(visible);
        }

        public static void SetVisibleByPath(this IGameObjectUtility baseUtility, string childPath, bool visible)
        {
            baseUtility.FindDirect(childPath).SetVisible(visible);
        }

        public static void SetVisible(this GameObject obj, bool visible)
        {
            if (obj is null || obj.activeSelf == visible)
                return;

            obj.SetActive(visible);
        }

        /// <summary>
        /// 从根对象开始获取路径
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetAbsolutePath(this GameObject obj)
        {
            var transform = obj.transform;
            if (transform == null)
                return "";

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        /// <summary>
        /// 从指定父级获取相对路径
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="relativeObj"></param>
        /// <returns></returns>
        public static string GetRelativePath(this GameObject obj, GameObject relativeObj)
        {
            var transform = obj.transform;
            if (transform == null)
                return "";

            if (relativeObj == null)
                return GetAbsolutePath(obj);

            var relativeToTransform = relativeObj.transform;
            string path = transform.name;
            Transform current = transform.parent;

            while (current != null && current != relativeToTransform)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}