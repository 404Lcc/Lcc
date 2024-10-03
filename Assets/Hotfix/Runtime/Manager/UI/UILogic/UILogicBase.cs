using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using LccModel;

namespace LccHotfix
{
    public abstract class UILogicBase : IUILogic
    {
        public GameObject gameObject { get { return WNode.gameObject; } }
        public Transform transform { get { return WNode.transform; } }




        public virtual void CloseWithAni()
        {
            Close();
        }
        private object Close()
        {
            return WNode.Close();
        }

        #region 接口函数

        public WNode WNode { get; set; }

        public virtual void OnStart()
        {
            AutoReference(transform);
            ShowView(gameObject);
        }

        public virtual void OnUpdate() { }

        public virtual bool EscCanClose()
        {
            return true;
        }

        public virtual void OnOpen(object[] paramsList) { }
        public virtual void OnResume() { }
        public virtual void OnPause() { }
        public virtual object OnClose() { return null; }
        public virtual void OnReset(object[] paramsList) { }
        public virtual void OnRemove()
        {
        }
        public virtual void OnSwitch(Action<bool> callback)
        {
            callback?.Invoke(true);
        }
        public virtual bool OnEscape(ref EscapeType escapeType)
        {
            escapeType = WNode.escapeType;
            if (EscapeType.SKIP_OVER == escapeType)
                return false;
            else
                return true;
        }

        public virtual void OnChildOpened(WNode child)
        {

        }

        public virtual bool OnChildClosed(WNode child)
        {
            return false;
        }

        public virtual bool OnChildRequireEscape(WNode child)
        {
            return true;
        }
        #endregion

        #region 自动索引
        public void AutoReference(Transform transform)
        {
            Dictionary<string, FieldInfo> fieldInfoDict = new Dictionary<string, FieldInfo>();
            FieldInfo[] fieldInfos = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
                AutoReference(transform, fieldInfoDict);
            }
        }
        public void AutoReference(Transform transform, Dictionary<string, FieldInfo> fieldInfoDict)
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


            Transform[] childrens = transform.GetComponentsInChildren<Transform>(true);

            foreach (Transform item in childrens)
            {
                string itemName = item.name.ToLower();
                if (fieldInfoDict.ContainsKey(itemName))
                {
                    if (fieldInfoDict[itemName].FieldType.Equals(typeof(GameObject)))
                    {
                        fieldInfoDict[itemName].SetValue(this, item.gameObject);
                    }
                    else if (fieldInfoDict[itemName].FieldType.Equals(typeof(Transform)))
                    {
                        fieldInfoDict[itemName].SetValue(this, item);
                    }
                    else
                    {
                        fieldInfoDict[itemName].SetValue(this, item.GetComponent(fieldInfoDict[itemName].FieldType));
                    }
                }
            }
        }
        public void AutoReference(GameObject gameObject)
        {
            AutoReference(gameObject.transform);
        }
        #endregion
        public void ShowView(GameObject gameObject, GameObject parent = null)
        {

            LccView view = gameObject.AddComponent<LccView>();
            view.className = GetType().Name;
            view.type = this;

            if (parent != null)
            {
                gameObject.transform.SetParent(parent.transform);
            }
        }

    }

}