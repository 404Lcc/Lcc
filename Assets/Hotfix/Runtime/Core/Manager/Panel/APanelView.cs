using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public abstract class APanelView<T> : AViewBase<T>, IPanelHandler where T : ViewModelBase
    {
        #region 自动引用
        public void AutoReference(Transform transform)
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
            for (int i = 0; i < transform.childCount; i++)
            {
                AutoReference(transform.GetChild(i), fieldInfoDict);
            }
        }
        public void AutoReference(GameObject gameObject)
        {
            AutoReference(gameObject.transform);
        }
        #endregion

        public virtual void ShowTopPanel(TopType topType, string title)
        {
            ViewModel.topData.topType = topType;
            ViewModel.topData.title = title;
            //刷新Top
            PanelManager.Instance.ShowPanel(PanelType.Top, new ShowPanelData(false, false, ViewModel.topData, false, false, false));
        }


        public virtual void OnInitData(Panel panel)
        {
            ViewModel.selfPanel = panel;
            ViewModel.InitTopData();
            AutoReference(panel.GameObject);
        }
        public virtual void OnInitComponent(Panel panel)
        {
        }
        public virtual void OnRegisterUIEvent(Panel panel)
        {
        }

        public virtual void OnShow(Panel panel, AObjectBase contextData = null)
        {
        }
        public virtual void OnHide(Panel panel)
        {
        }
        public virtual void OnBeforeUnload(Panel panel)
        {
        }

        public virtual void OnReset(Panel panel)
        {
        }

        public virtual bool IsReturn(Panel panel)
        {
            return false;
        }
    }
}