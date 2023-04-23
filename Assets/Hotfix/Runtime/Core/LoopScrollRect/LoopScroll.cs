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
        private LoopScrollRect loopScroll;
        private LoopScrollItemPool<View> loopScrollItemPool;
        public Dictionary<int, View> dict = new Dictionary<int, View>();
        public List<Data> dataList = new List<Data>();
        public bool needSelectWithClick = true;
        public Action<int, Data> selectAction = null;

        public int CurSelect { get; set; } = -1;

        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            loopScroll = (LoopScrollRect)datas[0];

            loopScrollItemPool = new LoopScrollItemPool<View>(this, 100, (GameObject)datas[1], loopScroll.content);

            loopScroll.GetObjectHandler = GetObject;
            loopScroll.ReturnObjectHandler = ReturnObject;
            loopScroll.ProvideDataHandler = ProvideData;

            ((GameObject)datas[1]).SetActive(false);
            if (datas.Length > 2)
            {
                selectAction = (Action<int, Data>)datas[2];
            }
        }
        #region 回调注册
        public GameObject GetObject(int index)
        {
            View item = loopScrollItemPool.GetOnPool();
            if (!dict.ContainsKey(index))
            {
                item.index = index;
                dict.Add(index, item);
            }
            return item.gameObject;
        }
        public void ReturnObject(Transform trans, int index)
        {
            if (dict.ContainsKey(index))
            {
                loopScrollItemPool.ReleaseOnPool(dict[index]);
                dict.Remove(index);
            }
            else
            {
                Debug.LogError("ReturnObject不存在" + index);
            }
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
        public void ClearList()
        {
            dataList.Clear();
            loopScroll.ClearCells();
        }
        public void SetDataList(List<Data> datas, int startItem = 0, bool fillViewRect = false, float contentOffset = 0)
        {
            dataList.Clear();
            dataList.AddRange(datas);
            loopScroll.totalCount = dataList.Count;
            loopScroll.RefillCells(startItem, fillViewRect, contentOffset);
        }
        public void SetDataList(List<Data> datas)
        {
            dataList.Clear();
            dataList.AddRange(datas);
            loopScroll.totalCount = dataList.Count;
            loopScroll.RefillCells();
        }

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