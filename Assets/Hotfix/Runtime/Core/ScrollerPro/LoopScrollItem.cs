using LccModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public class LoopScrollItem : AObjectBase
    {
        public int index = -1;


        public GroupBase groupBase;
        public int groupIndex => groupBase.groupIndex;
        public int groupStart => groupBase.groupStart;


        public GameObject gameObject;

        public ILoopScroll loopScroll => (ILoopScroll)Parent;

        public GameObject selectGo;
        public GameObject normalGo;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            gameObject = p1 as GameObject;
            AutoReference(gameObject);

            Button button = gameObject.GetComponent<Button>();
            if (button == null)
            {
                gameObject.AddComponent<Button>().onClick.AddListener(OnClick);
            }
            else
            {
                button.onClick.AddListener(OnClick);
            }

            OnInit();
        }

        public virtual void OnInit()
        {

        }

        public virtual void OnShow()
        {

        }

        public virtual void OnHide()
        {

        }

        public virtual void UpdateData(object obj)
        {
            OnItemSelect(loopScroll.CurSelect);
        }
        public virtual void OnClick()
        {
            loopScroll.SetSelect(index);
        }

        public virtual void OnItemSelect(int index)
        {
            UpdateSelectSpriteVisible(this.index == index);
        }

        private void UpdateSelectSpriteVisible(bool visible)
        {
            if (selectGo != null && selectGo.activeSelf != visible)
            {
                selectGo.SetActive(visible);
            }
            if (normalGo != null && normalGo.activeSelf == visible)
            {
                normalGo.SetActive(!visible);
            }
        }

        public void SetSize(Vector2 size)
        {
            loopScroll.SetSize(groupIndex, size);
        }
        public void SetSizeX(int x)
        {
            loopScroll.SetSizeX(groupIndex, x);
        }
        public void SetSizeY(int y)
        {
            loopScroll.SetSizeY(groupIndex, y);
        }

        #region 自动索引
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
        private void AutoReference(GameObject gameObject)
        {
            AutoReference(gameObject.transform);
        }
        #endregion
    }
}