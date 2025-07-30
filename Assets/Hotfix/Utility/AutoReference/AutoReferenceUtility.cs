using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LccHotfix
{
    public static class AutoReferenceUtility
    {
        #region 自动索引
        public static void AutoReference(object obj, Transform transform)
        {
            Dictionary<string, FieldInfo> fieldInfoDict = new Dictionary<string, FieldInfo>();
            FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type objectType = typeof(UnityEngine.Object);
            foreach (FieldInfo item in fieldInfos)
            {
                if (item.FieldType.IsSubclassOf(objectType))
                {
                    fieldInfoDict[item.Name.ToLower()] = item;
                }
            }

            if (fieldInfoDict.Count > 0)
            {
                AutoReference(obj, transform, fieldInfoDict);
            }
        }
        public static void AutoReference(object obj, Transform transform, Dictionary<string, FieldInfo> fieldInfoDict)
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
        public static void AutoReference(object obj, GameObject gameObject)
        {
            AutoReference(obj, gameObject.transform);
        }
        #endregion
    }
}