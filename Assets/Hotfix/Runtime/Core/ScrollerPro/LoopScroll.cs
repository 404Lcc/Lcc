using LccModel;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EnhancedUI.EnhancedScroller.EnhancedScroller;

namespace LccHotfix
{
    public interface ILoopScroll
    {
        int CurSelect { get; set; }
        void SetSelect(int index);

        void SetSize(int index, Vector2 size);
        void SetSizeX(int index, int x);
        void SetSizeY(int index, int y);
    }
    public class LoopScroll<Data, View> : AObjectBase, ILoopScroll where Data : new() where View : LoopScrollItem
    {
        private ScrollerPro loopScroll;
        private LoopScrollItemPool<View> loopScrollItemPool;

        public Dictionary<int, View> dict = new Dictionary<int, View>();
        public List<Data> dataList = new List<Data>();
        public bool needSelectWithClick = true;
        public Action<int, Data> selectAction = null;

        public GameObject itemPrefab;

        public GameObject groupPrefab;
        public Dictionary<int, Vector2> sizeDict = new Dictionary<int, Vector2>();
        public int CurSelect { get; set; } = -1;

        private Vector2 _itemSize = Vector2.zero;
        public Vector2 ItemSize
        {
            get
            {
                if (_itemSize == Vector2.zero)
                {
                    RectTransform rect = itemPrefab.transform as RectTransform;
                    _itemSize = rect.sizeDelta();
                }
                return _itemSize;
            }
        }

        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            loopScroll = (ScrollerPro)datas[0];
            itemPrefab = (GameObject)datas[1];
            loopScrollItemPool = new LoopScrollItemPool<View>(this, 100, itemPrefab, loopScroll.transform);

            loopScroll.GetObjectHandler = GetObject;
            loopScroll.ReturnObjectHandler = ReturnObject;
            loopScroll.ProvideDataHandler = ProvideData;
            loopScroll.GetGroupSizeHandler = GetGroupSize;
            loopScroll.GetDataCountHandler = GetDataCount;


            InitGroup(itemPrefab);
            if (datas.Length > 2)
            {
                selectAction = (Action<int, Data>)datas[2];
            }
        }
        public void InitGroup(GameObject itemPrefab)
        {
            itemPrefab.gameObject.SetActive(false);

            groupPrefab = new GameObject("groupPrefab");
            groupPrefab.SetActive(false);
            groupPrefab.transform.SetParent(loopScroll.transform);
            groupPrefab.AddComponent<RectTransform>();

            RectTransform groupRect = groupPrefab.transform as RectTransform;
            RectTransform itemRect = itemPrefab.transform as RectTransform;

            if (loopScroll.isGrid)
            {
                RectTransform loopScrollRect = loopScroll.transform as RectTransform;
                //GridLayoutGroup gridLayoutGroup = groupPrefab.AddComponent<GridLayoutGroup>();
                //gridLayoutGroup.spacing = new Vector2(loopScroll.Scroller.spacing, 0);
                //gridLayoutGroup.cellSize = itemRect.sizeDelta();
                groupRect.sizeDelta = new Vector2(loopScrollRect.sizeDelta().x, itemRect.sizeDelta().y);
            }
            else
            {
                groupRect.sizeDelta = itemRect.sizeDelta();
            }

            GroupBase groupBase = groupPrefab.AddComponent<GroupBase>();
            groupBase.InitGroup(loopScroll, itemPrefab.transform);
            loopScroll.groupPrefab = groupBase;
        }
        #region 回调注册
        public void GetObject(GroupBase groupBase, int index)
        {
            if (!dict.ContainsKey(index))
            {
                View item = loopScrollItemPool.GetOnPool();
                item.index = index;
                item.gameObject.name = index.ToString();

                item.groupBase = groupBase;

                RectTransform rect = item.gameObject.transform as RectTransform;
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = sizeDict[index];
                rect.SetParent(groupBase.transform);

                
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;

                int currentIndex = index % loopScroll.NumberOfCellsPerRow;
                rect.anchoredPosition = new Vector2((sizeDict[index].x + loopScroll.Scroller.spacing) * currentIndex, 0);
                LogUtil.Debug((index % loopScroll.NumberOfCellsPerRow).ToString());

                item.OnShow();
                dict.Add(index, item);


            }
            else
            {
                Debug.LogError("GetObject不存在" + index);
            }
        }
        public void ReturnObject(int index)
        {
            if (dict.ContainsKey(index))
            {
                dict[index].OnHide();
                loopScrollItemPool.ReleaseOnPool(dict[index]);
                dict.Remove(index);
            }
            else
            {
                Debug.LogError("ReturnObject不存在" + index);
            }
        }
        public void ProvideData(int index)
        {
            if (dict.ContainsKey(index))
            {
                dict[index].UpdateData(dataList[index]);
            }
            else
            {
                Debug.LogError("ProvideData不存在" + index);
            }
        }

        public int GetGroupSize(int dataIndex)
        {
            int max = loopScroll.GetDataCountHandler();
            var groupStart = dataIndex * loopScroll.NumberOfCellsPerRow;

            Vector2 maxSize = Vector2.zero;
            for (int i = 0; i < loopScroll.NumberOfCellsPerRow; i++)
            {
                if (groupStart + i >= max) continue;
                var index = groupStart + i;
                if (sizeDict.ContainsKey(index))
                {
                    var temp = sizeDict[index];
                    if (temp.x > maxSize.x)
                    {
                        maxSize.x = temp.x;
                    }
                    if (temp.y > maxSize.y)
                    {
                        maxSize.y = temp.y;
                    }
                }
            }
            var groupSize = loopScroll.Scroller.scrollDirection == ScrollDirectionEnum.Vertical ? maxSize.y : maxSize.x;
            return (int)groupSize;
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

        #region 设置大小
        public void SetSize(int index, Vector2 size)
        {
            if (sizeDict.ContainsKey(index))
            {
                sizeDict[index] = size;
            }
            var groupIndex = index / loopScroll.NumberOfCellsPerRow;

            var cellPosition = loopScroll.Scroller.GetScrollPositionForCellViewIndex(groupIndex, CellViewPositionEnum.Before);
            var tweenCellOffset = cellPosition - loopScroll.Scroller.ScrollPosition;
            loopScroll.IgnoreLoopJump(true);
            loopScroll.Scroller.ReloadData();
            cellPosition = loopScroll.Scroller.GetScrollPositionForCellViewIndex(groupIndex, CellViewPositionEnum.Before);
            loopScroll.Scroller.SetScrollPositionImmediately(cellPosition - tweenCellOffset);
            loopScroll.IgnoreLoopJump(false);
        }
        public void SetSizeX(int index, int x)
        {
            if (sizeDict.ContainsKey(index))
            {
                var size = sizeDict[index];
                size.x = x;
                SetSize(index, size);
            }
        }
        public void SetSizeY(int index, int y)
        {
            if (sizeDict.ContainsKey(index))
            {
                var size = sizeDict[index];
                size.y = y;
                SetSize(index, size);
            }
        }

        #endregion

        #region 预加载大小
        public void PreloadSize(int index, Vector2 size)
        {
            if (sizeDict.ContainsKey(index))
            {
                sizeDict[index] = size;
            }
            else
            {
                sizeDict.Add(index, size);
            }
        }
        public void PreloadSizeX(int index, int x)
        {
            if (sizeDict.ContainsKey(index))
            {
                sizeDict[index] = new Vector2(x, ItemSize.y);
            }
            else
            {
                sizeDict.Add(index, new Vector2(x, ItemSize.y));
            }
        }
        public void PreloadSizeY(int index, int y)
        {
            if (sizeDict.ContainsKey(index))
            {
                sizeDict[index] = new Vector2(ItemSize.x, y);
            }
            else
            {
                sizeDict.Add(index, new Vector2(ItemSize.x, y));
            }
        }
        #endregion


        public void RefershData()
        {
            loopScroll.RefershData();
        }
        public void ClearList()
        {
            loopScroll.ClearAll();
            dict.Clear();
            dataList.Clear();
            sizeDict.Clear();

            GameObject.Destroy(groupPrefab);
        }

        public void Refill(List<Data> datas, int startItem = 0)
        {
            dataList.Clear();
            if (datas != null)
            {
                dataList.AddRange(datas);
            }

            for (int i = 0; i < datas.Count/*(datas.Count / loopScroll.NumberOfCellsPerRow) + 1*/; i++)
            {
                if (!sizeDict.ContainsKey(i))
                {
                    sizeDict.Add(i, ItemSize);
                }
            }

            loopScroll.ReloadData();

            if (startItem > 0)
            {
                loopScroll.JumpToDataIndex(startItem / loopScroll.NumberOfCellsPerRow);
            }
        }

        //public void AddData(Data data)
        //{
        //    dataList.Add(data);
        //    loopScroll.ReloadData();
        //}

        //public void AddDataList(List<Data> datas)
        //{
        //    dataList.AddRange(datas);
        //    loopScroll.ReloadData();
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