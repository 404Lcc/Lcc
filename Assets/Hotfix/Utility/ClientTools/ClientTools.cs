using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

public class ItemBase
{
    private GameObject _gameObject;
    public GameObject GameObject 
    {
        get 
        {
            return _gameObject;
        }
        set 
        {
            if (value == null)
                return;
            _gameObject = value;
            ClientTools.AutoReference(_gameObject, this);
        }
    }
    public Transform Transform
    {
        get
        {
            if (_gameObject != null)
            {
                return _gameObject.transform;
            }
            return null;
        }
    }

    public virtual void Show() 
    {

    }
    public virtual void Hide() 
    {

    }
}

/// <summary>
/// 客户端工具类
/// 此类禁止写入跟游戏功能有关的逻辑
/// </summary>
public class ClientTools
{
    #region Item工具

    /// <summary>
    /// 复用item工具
    /// </summary>
    /// <param name="cachedItems">存储item的列表</param>
    /// <param name="parent">将要放置item的父节点</param>
    /// <param name="item">要添加的item</param>
    /// <returns></returns>
    public static GameObject GetOneCached(List<GameObject> cachedItems, GameObject parent, GameObject item)
    {
        if (cachedItems == null)
        {
            cachedItems = new List<GameObject>();
        }

        for (int i = 0; i < cachedItems.Count; i++)
        {
            if (cachedItems[i] == null)
                continue;
            if (cachedItems[i].activeSelf == false)
            {
                cachedItems[i].SetActive(true);
                return cachedItems[i];
            }
        }

        if (parent != null && item != null)
        {
            GameObject go = GameObject.Instantiate(item);
            ResetTransform(go.transform, parent.transform);
            go.SetActive(true);
            cachedItems.Add(go);
            return go;
        }

        return null;
    }

    public static T GetOneCached<T>(List<T> cachedItems, GameObject parent, GameObject item) where T : ItemBase, new()
    {
        if (cachedItems == null)
        {
            cachedItems = new List<T>();
        }

        for (int i = 0; i < cachedItems.Count; i++)
        {
            if (cachedItems[i] == null)
                continue;
            if (cachedItems[i].GameObject == null)
                continue;

            if (cachedItems[i].GameObject.activeSelf == false)
            {
                T script = cachedItems[i]; //要求类型和item一致，避免继承子类判断错误
                if (script.GetType() != typeof(T)) continue;
                cachedItems[i].GameObject.SetActive(true);
                cachedItems[i].Show();
                return cachedItems[i];
            }
        }

        if (parent != null && item != null)
        {
            GameObject go = GameObject.Instantiate(item);
            ResetTransform(go.transform, parent.transform);
            go.SetActive(true);

            T script = new T();
            script.GameObject = go;
            script.Show();
            cachedItems.Add(script);
            return script;
        }

        return null;
    }

    /// <summary>
    /// 隐藏item存储列表内的item
    /// </summary>
    /// <param name="cachedItems"></param>
    public static void SetCachedItemHide(List<GameObject> cachedItems)
    {
        if (cachedItems == null)
        {
            return;
        }

        for (int i = 0; i < cachedItems.Count; i++)
        {
            if (cachedItems[i] != null)
            {
                cachedItems[i].SetActive(false);
            }
        }
    }

    public static void SetCachedItemHide<T>(List<T> cachedItems) where T : ItemBase
    {
        if (cachedItems == null)
        {
            return;
        }

        for (int i = 0; i < cachedItems.Count; i++)
        {
            if (cachedItems[i] != null && cachedItems[i].GameObject != null)
            {
                cachedItems[i].GameObject.SetActive(false);
                cachedItems[i].Hide();
            }
        }
    }

    #endregion

    #region 组件工具

    /// <summary>
    /// getComponent没有会自动加上一个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns></returns>
    public static T ForceGetComponent<T>(GameObject go, bool isAutoAdd = true) where T : Component
    {
        T com = go.GetComponent<T>();
        if (com == null && isAutoAdd)
            com = go.AddComponent<T>();
        return com;
    }

    public static GameObject GetChild(GameObject go, string childname)
    {
        if (go == null || string.IsNullOrEmpty(childname))
        {
            return null;
        }

        Transform parentTransform = go.transform;
        Transform childTransform = null;

        childTransform = parentTransform.Find(childname);
        if (childTransform == null)
        {
            return null;
        }

        return childTransform.gameObject;
    }

    public static Transform GetChildTransform(GameObject go, string childname)
    {
        if (go == null || string.IsNullOrEmpty(childname))
        {
            return null;
        }

        Transform childTransform = go.transform.Find(childname);
        return childTransform;
    }

    public static T GetComponent<T>(GameObject go) where T : Component
    {
        if (go == null)
            return null;
        T componet = go.GetComponent(typeof(T)) as T;
        if (componet == null)
            componet = go.AddComponent(typeof(T)) as T;
        return componet;
    }

    public static T GetChildComponent<T>(GameObject go, string childname) where T : Component
    {
        if (go == null || string.IsNullOrEmpty(childname))
        {
            return null;
        }

        GameObject child = GetChild(go, childname);
        if (child != null)
        {
            T component = child.GetComponent(typeof(T)) as T;
            return component;
        }
        else
        {
            return null;
        }
    }

    public static T AddChildComponent<T>(GameObject go, string childname) where T : Component
    {
        if (go == null || string.IsNullOrEmpty(childname))
        {
            return null;
        }

        GameObject child = GetChild(go, childname);
        if (child != null)
        {
            T component = child.GetComponent<T>();
            if (component == null)
            {
                return child.AddComponent<T>();
            }
            else
            {
                return component;
            }
        }
        else
        {
            return null;
        }
    }

    public static GameObject GetParent(GameObject go)
    {
        if (go == null)
        {
            return null;
        }

        Transform transform = go.transform.parent;
        if (transform == null)
        {
            return null;
        }

        return transform.gameObject;
    }

    #endregion

    #region Transform工具

    public static void ResetTransform(Transform trans, Transform parent)
    {
        if (trans == null)
            return;
        trans.SetParent(parent);
        trans.localScale = Vector3.one;
        trans.localRotation = Quaternion.identity;
        trans.localPosition = Vector3.zero;
    }

    public static void ResetTransform(Transform trans, Transform parent, Vector3 scale)
    {
        trans.SetParent(parent);
        trans.localScale = scale;
        trans.localRotation = Quaternion.identity;
        trans.localPosition = Vector3.zero;
    }

    public static void ResetTransform(Transform trans, Transform parent, Vector3 scale, Vector3 pos)
    {
        trans.SetParent(parent);
        trans.localScale = scale;
        trans.localRotation = Quaternion.identity;
        trans.localPosition = pos;
    }

    public static void ResetTransform(Transform trans, Transform parent, Vector3 scale, Vector3 pos, Quaternion rot)
    {
        trans.SetParent(parent);
        trans.localScale = scale;
        trans.localRotation = rot;
        trans.localPosition = pos;
    }

    public static void ResetRectTransfrom(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;
        rectTransform.pivot = Vector3.one * 0.5f;
        rectTransform.anchorMax = Vector3.one * 0.5f;
        rectTransform.anchorMin = Vector3.one * 0.5f;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localPosition = Vector3.zero;
    }

    #endregion

    #region 自动索引

    public static void AutoReference(Transform transform, object obj)
    {
        Dictionary<string, FieldInfo> fieldInfoDict = new Dictionary<string, FieldInfo>();
        FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Type objectType = typeof(Object);
        foreach (FieldInfo item in fieldInfos)
        {
            if (item.FieldType.IsSubclassOf(objectType))
            {
                fieldInfoDict[item.Name.ToLower()] = item;
            }
        }

        if (fieldInfoDict.Count > 0)
        {
            void AutoReference(Transform transform, Dictionary<string, FieldInfo> fieldInfoDict)
            {
                string name = transform.name.ToLower();
                if (fieldInfoDict.ContainsKey(name))
                {
                    if (fieldInfoDict[name].FieldType.Equals(typeof(GameObject)))
                    {
                        fieldInfoDict[name].SetValue(obj, transform.gameObject);
                    }
                    else if (fieldInfoDict[name].FieldType.Equals(typeof(Transform)))
                    {
                        fieldInfoDict[name].SetValue(obj, transform);
                    }
                    else
                    {
                        fieldInfoDict[name].SetValue(obj, transform.GetComponent(fieldInfoDict[name].FieldType));
                    }
                }


                Transform[] childrens = transform.GetComponentsInChildren<Transform>(true);

                foreach (Transform item in childrens)
                {
                    string itemName = item.name.ToLower();
                    if (fieldInfoDict.ContainsKey(itemName))
                    {
                        if (fieldInfoDict[itemName].FieldType.Equals(typeof(GameObject)))
                        {
                            fieldInfoDict[itemName].SetValue(obj, item.gameObject);
                        }
                        else if (fieldInfoDict[itemName].FieldType.Equals(typeof(Transform)))
                        {
                            fieldInfoDict[itemName].SetValue(obj, item);
                        }
                        else
                        {
                            fieldInfoDict[itemName].SetValue(obj, item.GetComponent(fieldInfoDict[itemName].FieldType));
                        }
                    }
                }
            }

            AutoReference(transform, fieldInfoDict);
        }
    }

    public static void AutoReference(GameObject gameObject, object obj)
    {
        AutoReference(gameObject.transform, obj);
    }

    #endregion

    #region 坐标转换

    /// <summary>
    /// 世界坐标转屏幕坐标
    /// </summary>
    public static Vector2 World2Screen(Vector3 worldPos, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        return camera.WorldToScreenPoint(worldPos);
    }

    /// <summary>
    /// 屏幕坐标转世界坐标
    /// </summary>
    public static Vector3 Screen2World(Vector3 screenPos, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        return camera.ScreenToWorldPoint(screenPos);
    }

    /// <summary>
    /// 世界坐标转视口坐标
    /// </summary>
    public static Vector2 World2Viewport(Vector3 worldPos, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        return camera.WorldToViewportPoint(worldPos);
    }

    /// <summary>
    /// 视口坐标转世界坐标
    /// </summary>
    public static Vector3 Viewport2World(Vector3 viewPos, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        return camera.ViewportToWorldPoint(viewPos);
    }

    /// <summary>
    /// 屏幕坐标转视口坐标
    /// </summary>
    public static Vector2 Screen2Viewport(Vector2 screenPos, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        return camera.ScreenToViewportPoint(screenPos);
    }

    /// <summary>
    /// 视口坐标转屏幕坐标
    /// </summary>
    public static Vector2 Viewport2Screen(Vector2 viewPos, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        return camera.ViewportToScreenPoint(viewPos);
    }

    /// <summary>
    /// 屏幕坐标转UI局部坐标
    /// </summary>
    public static Vector2 Screen2UILocal(Vector2 screenPos, RectTransform rect, Camera uiCamera = null)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, uiCamera, out Vector2 uiLocalPos);
        return uiLocalPos;
    }

    /// <summary>
    /// 世界坐标转UI局部坐标
    /// </summary>
    public static Vector2 World2UILocal(Vector3 worldPos, RectTransform rect, Camera worldCamera = null, Camera uiCamera = null)
    {
        Vector2 screenPos = World2Screen(worldPos, worldCamera);
        Vector2 uiLocalPos = Screen2UILocal(screenPos, rect, uiCamera);
        return uiLocalPos;
    }

    /// <summary>
    /// UI世界坐标转屏幕坐标
    /// </summary>
    public static Vector2 UIWorld2Screen(Vector3 worldPos, Camera uiCamera = null)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);
        return screenPos;
    }

    #endregion
}