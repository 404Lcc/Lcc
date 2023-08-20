using LccModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public interface ILoopScrollSelect
    {
        int CurSelect { get; set; }
        void SetSelect(int index);
    }
    public class LoopScroll<Data, View> : AObjectBase, ILoopScrollSelect where Data : new() where View : LoopScrollItem
    {
        private ScrollerPro loopScroll;
        private LoopScrollItemPool<View> loopScrollItemPool;

        public Dictionary<int, View> dict = new Dictionary<int, View>();
        public List<Data> dataList = new List<Data>();
        public bool needSelectWithClick = true;
        public Action<int, Data> selectAction = null;

        public int CurSelect { get; set; } = -1;

        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            loopScroll = (ScrollerPro)datas[0];

            loopScrollItemPool = new LoopScrollItemPool<View>(this, 100);

            loopScroll.GetObjectHandler = GetObject;
            loopScroll.ReturnObjectHandler = ReturnObject;
            loopScroll.ProvideDataHandler = ProvideData;
            loopScroll.GetDataCountHandler = GetDataCount;

            var itemPrefab = (GameObject)datas[1];
            InitGroup(itemPrefab);
            if (datas.Length > 2)
            {
                selectAction = (Action<int, Data>)datas[2];
            }
        }
        public void InitGroup(GameObject itemPrefab)
        {
            itemPrefab.gameObject.SetActive(false);

            GameObject group = new GameObject("groupPrefab");
            group.SetActive(false);
            group.transform.SetParent(loopScroll.transform);
            group.AddComponent<RectTransform>();

            RectTransform groupRect = group.transform as RectTransform;
            RectTransform itemRect = itemPrefab.transform as RectTransform;

            if (loopScroll.isGrid)
            {
                RectTransform loopScrollRect = loopScroll.transform as RectTransform;
                GridLayoutGroup gridLayoutGroup = group.AddComponent<GridLayoutGroup>();
                gridLayoutGroup.spacing = new Vector2(loopScroll.Scroller.spacing, 0);
                gridLayoutGroup.cellSize = itemRect.sizeDelta();
                groupRect.sizeDelta = new Vector2(loopScrollRect.sizeDelta().x, itemRect.sizeDelta().y);
            }
            else
            {
                groupRect.sizeDelta = itemRect.sizeDelta();
            }

            GroupBase groupBase = group.AddComponent<GroupBase>();
            groupBase.InitGroup(loopScroll, itemPrefab.transform);
            loopScroll.groupPrefab = groupBase;
        }
        #region 回调注册
        public void GetObject(Transform trans, int index)
        {
            LogUtil.Debug("GetObject"+ index);
            if (!dict.ContainsKey(index))
            {
                View item = loopScrollItemPool.GetOnPool();
                item.index = index;
                item.gameObject = trans.gameObject;
                dict.Add(index, item);
            }
            else
            {
            }
        }
        public void ReturnObject(Transform trans, int index)
        {
            LogUtil.Debug("ReturnObject" + index);
            //if (dict.ContainsKey(index))
            //{
            //    loopScrollItemPool.ReleaseOnPool(dict[index]);
            //    dict.Remove(index);
            //}
            //else
            //{
            //    Debug.LogError("ReturnObject不存在" + index);
            //}
        }
        public void ProvideData(Transform transform, int index)
        {
            transform.name = index.ToString();
            if (dict.ContainsKey(index))
            {
                dict[index].UpdateData(dataList[index]);
            }
            else
            {
                Debug.LogError("ProvideData不存在" + index);
            }
        }
        public int GetDataCount()
        {
            return dataList.Count;
        }
        #endregion
        public void SetSelect(int index)
        {
            if (needSelectWithClick)
            {
                CurSelect = index;
                if (selectAction != null && index >= 0)
                {
                    selectAction(index, dataList[index]);
                }
                if (dict.Count > 0)
                {
                    foreach (var item in dict.Values)
                    {
                        item.OnItemSelect(index);
                    }
                }
            }
        }
        public void RefershData()
        {
            loopScroll.RefershData();
        }
        //public void ClearList()
        //{
        //    dict.Clear();
        //    dataList.Clear();
        //    loopScroll.ClearCells();
        //}

        public void Refill(List<Data> datas, int startItem = 0, bool fillViewRect = false, float contentOffset = 0)
        {
            dataList.Clear();
            if (datas != null)
            {
                dataList.AddRange(datas);
            }
            loopScroll.ReloadData();
        }

        //public void SetDataListAndRefreshList(List<Data> datas)
        //{
        //    dataList.Clear();
        //    if (datas != null)
        //    {
        //        dataList.AddRange(datas);
        //    }
        //    loopScroll.totalCount = dataList.Count;
        //    RefreshList(false);
        //}

        //public void AddData(Data data, bool setPosition = false)
        //{
        //    dataList.Add(data);
        //    loopScroll.totalCount = dataList.Count;
        //    if (setPosition)
        //    {
        //        loopScroll.RefillCells(dataList.Count);
        //    }
        //    else
        //    {
        //        RefreshList(true);
        //    }
        //}

        //public void AddDataList(List<Data> datas, bool setPosition = false)
        //{
        //    dataList.AddRange(datas);
        //    loopScroll.totalCount = dataList.Count;
        //    if (setPosition)
        //    {
        //        loopScroll.RefillCells(dataList.Count);
        //    }
        //    else
        //    {
        //        RefreshList(true);
        //    }
        //}

        //public void RefreshList(bool resize = false)
        //{
        //    loopScroll.RefreshCells(resize);
        //}

        public View GetItem(int idx)
        {
            if (dict.TryGetValue(idx, out View view))
            {
                return view;
            }
            return null;
        }
    }
}