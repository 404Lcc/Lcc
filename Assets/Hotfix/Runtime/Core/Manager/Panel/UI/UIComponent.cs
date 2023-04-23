using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public class UIComponent
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public Canvas canvas;

        public List<Canvas> canvasList;

        //public List<UIItem> itemList;

        #region 自动引用
        private void AutoReference(Transform transform)
        {
            Dictionary<string, FieldInfo> fieldInfoDict = new Dictionary<string, FieldInfo>();
            FieldInfo[] fieldInfos = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
                AutoReference(transform, fieldInfoDict);
            }
        }
        private void AutoReference(Transform transform, Dictionary<string, FieldInfo> fieldInfoDict)
        {
            string name = transform.name.ToLower();
            if (fieldInfoDict.ContainsKey(name))
            {
                if (fieldInfoDict[name].FieldType.Equals(typeof(GameObject)))
                {
                    fieldInfoDict[name].SetValue(this, transform.gameObject);
                }
                else if (fieldInfoDict[name].FieldType.Equals(typeof(Transform)))
                {
                    fieldInfoDict[name].SetValue(this, transform);
                }
                else
                {
                    fieldInfoDict[name].SetValue(this, transform.GetComponent(fieldInfoDict[name].FieldType));
                }
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                AutoReference(transform.GetChild(i), fieldInfoDict);
            }
        }
        private void AutoReference(GameObject gameObject)
        {
            AutoReference(gameObject.transform);
        }
        #endregion

        private void GetItemComponent(Transform transform)
        {
            Canvas canvas = transform.gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = transform.gameObject.AddComponent<Canvas>();
                transform.gameObject.AddComponent<GraphicRaycaster>();
            }
            if (canvas != this.canvas)
            {
                canvasList.Add(canvas);
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                GetItemComponent(transform.GetChild(i));
            }
        }

        protected void InitComponent(GameObject gameObject)
        {
            this.gameObject = gameObject;
            rectTransform = (RectTransform)gameObject.transform;
            canvas = gameObject.GetComponent<Canvas>();
            canvasList = new List<Canvas>();
            //itemList = new List<UIItem>();

            AutoReference(gameObject);
            //GetItemComponent(rectTransform);
        }

        //protected T InitItem<T>(GameObject gameObject) where T : UIItem, new()
        //{
        //    T item = new T();
        //    item.OnInitComponent(gameObject);
        //    itemList.Add(item);
        //    return item;
        //}

        //刷新层级
        protected void UpdateDepth()
        {
            //int depth = canvas.sortingOrder;
            //for (int i = 1; i <= canvasList.Count; i++)
            //{
            //    canvasList[i - 1].overrideSorting = true;
            //    canvasList[i - 1].sortingOrder = depth + i;
            //}
        }
    }
}