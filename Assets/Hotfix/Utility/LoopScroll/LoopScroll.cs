using EnhancedUI.EnhancedScroller;
using System;
using System.Collections.Generic;
using LccModel;
using UnityEngine;
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

    public class LoopScroll<Data, Item> : ILoopScroll where Data : new() where Item : LoopScrollItem, new()
    {
        private ScrollerPro _scrollerPro;
        private LoopScrollPool _pool;

        private Dictionary<int, Item> _itemDict = new Dictionary<int, Item>();
        private List<Data> _dataList = new List<Data>();

        //是否需要选择
        private bool _needSelectWithClick = true;

        /// 点击事件
        /// </summary>
        private Action<object> _clickFunc;

        /// <summary>
        /// 选择事件
        /// </summary>
        private Action<int, Data> _selectAction = null;

        private GameObject _itemPrefab;

        private GameObject _groupPrefab;
        private Dictionary<int, Vector2> _sizeDict = new Dictionary<int, Vector2>();



        public int CurSelect { get; set; } = -1;

        public int StartIndex => _scrollerPro.StartDataIndex;
        public int EndIndex => _scrollerPro.EndDataIndex;
        public ScrollerPro ScrollerPro => _scrollerPro;

        public ScrollerSnappedDelegate ScrollerSnapped
        {
            set { _scrollerPro.scrollerSnapped = value; }
        }

        public ScrollerScrolledDelegate ScrollerScrolled
        {
            set { _scrollerPro.scrollerScrolled = value; }
        }


        public Vector2 ItemSize => (_itemPrefab.transform as RectTransform).SizeDelta();

        public LoopScroll(ScrollerPro scrollerPro, Action<int, Data> selectAction = null)
        {
            _scrollerPro = scrollerPro;
            _scrollerPro.Init();

            _itemPrefab = scrollerPro.transform.Find("item").gameObject;
            _itemPrefab.gameObject.SetActive(false);

            _pool = new LoopScrollPool();

            _scrollerPro.GetObjectHandler = GetObject;
            _scrollerPro.ReturnObjectHandler = ReturnObject;
            _scrollerPro.ProvideDataHandler = ProvideData;
            _scrollerPro.GetGroupSizeHandler = GetGroupSize;
            _scrollerPro.GetDataCountHandler = GetDataCount;

            if (selectAction != null)
            {
                this._selectAction = selectAction;
            }

            SetGroupPrefab();
        }

        public void SetGroupPrefab()
        {
            if (_scrollerPro.groupPrefab != null)
            {
                return;
            }

            var obj = _scrollerPro.transform.Find("groupPrefab");
            if (obj != null)
            {
                _scrollerPro.groupPrefab = obj.GetComponent<GroupBase>();
                return;
            }

            _groupPrefab = new GameObject("groupPrefab", typeof(RectTransform));
            _groupPrefab.SetActive(false);
            _groupPrefab.transform.SetParent(_scrollerPro.transform);

            RectTransform groupRect = _groupPrefab.transform as RectTransform;
            RectTransform itemRect = _itemPrefab.transform as RectTransform;

            if (_scrollerPro.isGrid)
            {
                RectTransform loopScrollRect = _scrollerPro.transform as RectTransform;
                if (_scrollerPro.scrollDirection == ScrollDirectionEnum.Vertical)
                {
                    groupRect.sizeDelta = new Vector2(loopScrollRect.SizeDelta().x, itemRect.SizeDelta().y);
                }
                else
                {
                    groupRect.sizeDelta = new Vector2(itemRect.SizeDelta().x, loopScrollRect.SizeDelta().y);
                }
            }
            else
            {
                groupRect.sizeDelta = itemRect.SizeDelta();
            }

            GroupBase groupBase = _groupPrefab.AddComponent<GroupBase>();
            groupBase.InitGroup(_scrollerPro, _itemPrefab.transform);
            _scrollerPro.groupPrefab = groupBase;
        }

        //动态修改isGrid的时候需要调用这个
        public void RefreshGroup()
        {
            RectTransform groupRect = _groupPrefab.transform as RectTransform;
            RectTransform itemRect = _itemPrefab.transform as RectTransform;

            if (_scrollerPro.isGrid)
            {
                RectTransform loopScrollRect = _scrollerPro.transform as RectTransform;

                if (_scrollerPro.scrollDirection == ScrollDirectionEnum.Vertical)
                {
                    groupRect.sizeDelta = new Vector2(loopScrollRect.SizeDelta().x, itemRect.SizeDelta().y);
                }
                else
                {
                    groupRect.sizeDelta = new Vector2(itemRect.SizeDelta().x, loopScrollRect.SizeDelta().y);
                }
            }
            else
            {
                groupRect.sizeDelta = itemRect.SizeDelta();
            }

            GroupBase groupBase = _groupPrefab.GetComponent<GroupBase>();
            groupBase.InitGroup(_scrollerPro, _itemPrefab.transform);
        }

        #region 回调注册

        public void GetObject(GroupBase groupBase, int index, int currentLength)
        {
            if (!_itemDict.ContainsKey(index))
            {
                Item item = _pool.Get<Item>();
                if (item.gameObject == null)
                {
                    GameObject obj = GameObject.Instantiate(_itemPrefab);
                    RectTransform objRect = obj.transform as RectTransform;
                    objRect.anchoredPosition = Vector3.zero;
                    objRect.localPosition = Vector3.zero;
                    objRect.localRotation = Quaternion.identity;
                    objRect.localScale = Vector3.one;
                    item.Init(this, obj);
                }

                item.gameObject.SetActive(true);
                item.index = index;
                item.gameObject.name = index.ToString();
                item.groupBase = groupBase;

                //重置位置
                RectTransform rect = item.gameObject.transform as RectTransform;
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = _sizeDict[index];
                rect.SetParent(groupBase.transform);

                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;

                //当前组的第几个索引
                int currentIndex = index % _scrollerPro.NumberOfCellsPerRow;

                //当前组最后那个索引
                int maxIndex = currentLength;
                var endPos = Vector2.zero;


                if (_scrollerPro.scrollDirection == ScrollDirectionEnum.Vertical)
                {
                    //先只考虑item大小都一样的情况，直接用第0个当默认大小
                    endPos = new Vector2((_sizeDict[groupBase.groupStart].x + _scrollerPro.Scroller.spacing) * (groupBase.gridCount - 1), 0);

                    var sizeX = (_scrollerPro.Container.SizeDelta().x - (endPos.x + _sizeDict[maxIndex].x)); //左上对齐
                    var halfSizeX = sizeX / 2;

                    switch (_scrollerPro.pivotHorizontal)
                    {
                        case PivotHorizontal.Left:
                            rect.anchoredPosition = new Vector2((_sizeDict[index].x + _scrollerPro.Scroller.spacing) * currentIndex, 0);
                            break;
                        case PivotHorizontal.Center:
                            rect.anchoredPosition = new Vector2((_sizeDict[index].x + _scrollerPro.Scroller.spacing) * currentIndex + halfSizeX, 0);
                            break;
                        case PivotHorizontal.Middle:
                            endPos = Vector2.zero;
                            //因为每个sizeDict大小可能不一样所以遍历加一下
                            for (int i = groupBase.groupStart; i < maxIndex; i++)
                            {
                                endPos.x += (_sizeDict[i].x + _scrollerPro.Scroller.spacing);
                            }

                            sizeX = (_scrollerPro.Container.SizeDelta().x - (endPos.x + _sizeDict[maxIndex].x)); //左上对齐
                            halfSizeX = sizeX / 2;
                            rect.anchoredPosition = new Vector2((_sizeDict[index].x + _scrollerPro.Scroller.spacing) * currentIndex + halfSizeX, 0);
                            break;
                        case PivotHorizontal.Right:
                            rect.anchoredPosition = new Vector2((_sizeDict[index].x + _scrollerPro.Scroller.spacing) * currentIndex + sizeX, 0);
                            break;
                    }
                }

                if (_scrollerPro.scrollDirection == ScrollDirectionEnum.Horizontal)
                {
                    //先只考虑item大小都一样的情况，直接用第0个当默认大小
                    endPos = new Vector2(0, (_sizeDict[groupBase.groupStart].y + _scrollerPro.Scroller.spacing) * (groupBase.gridCount - 1));

                    var sizeY = (_scrollerPro.Container.SizeDelta().y - (endPos.y + _sizeDict[maxIndex].y)); //左上对齐
                    var halfSizeY = sizeY / 2;

                    switch (_scrollerPro.pivotVertical)
                    {
                        case PivotVerticle.Top:
                            rect.anchoredPosition = new Vector2(0, -(_sizeDict[index].y + _scrollerPro.Scroller.spacing) * currentIndex);
                            break;
                        case PivotVerticle.Center:
                            rect.anchoredPosition = new Vector2(0, -((_sizeDict[index].y + _scrollerPro.Scroller.spacing) * currentIndex + halfSizeY));
                            break;
                        case PivotVerticle.Middle:
                            endPos = Vector2.zero;
                            //因为每个sizeDict大小可能不一样所以遍历加一下
                            for (int i = groupBase.groupStart; i < maxIndex; i++)
                            {
                                endPos.y += (_sizeDict[i].y + _scrollerPro.Scroller.spacing);
                            }

                            sizeY = (_scrollerPro.Container.SizeDelta().y - (endPos.y + _sizeDict[maxIndex].y)); //左上对齐
                            halfSizeY = sizeY / 2;

                            rect.anchoredPosition = new Vector2(0, -((_sizeDict[index].y + _scrollerPro.Scroller.spacing) * currentIndex + halfSizeY));
                            break;
                        case PivotVerticle.Bottom:
                            rect.anchoredPosition = new Vector2(0, -((_sizeDict[index].y + _scrollerPro.Scroller.spacing) * currentIndex + sizeY));
                            break;
                    }
                }

                item.OnShow();
                _itemDict.Add(index, item);
            }
        }

        private void ReturnObject(int index)
        {
            if (_itemDict.ContainsKey(index))
            {
                _itemDict[index].gameObject.SetActive(false);
                _pool.Release(_itemDict[index]);
                _itemDict.Remove(index);
            }
        }

        private void ProvideData(int index)
        {
            if (_itemDict.ContainsKey(index))
            {
                _itemDict[index].UpdateData(_dataList[index]);
            }
            else
            {
                Debug.LogError("ProvideData不存在" + index);
            }
        }

        //返回最大x或者y 如果是垂直列表就返回y， 垂直列表 grid排布的时候x可以随便调整
        private int GetGroupSize(int dataIndex)
        {
            int max = _scrollerPro.GetDataCountHandler();
            var groupStart = dataIndex * _scrollerPro.NumberOfCellsPerRow;

            Vector2 maxSize = Vector2.zero;
            for (int i = 0; i < _scrollerPro.NumberOfCellsPerRow; i++)
            {
                if (groupStart + i >= max) continue;
                var index = groupStart + i;
                if (_sizeDict.ContainsKey(index))
                {
                    var temp = _sizeDict[index];
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

            var groupSize = _scrollerPro.Scroller.scrollDirection == ScrollDirectionEnum.Vertical ? maxSize.y : maxSize.x;
            return (int)groupSize;
        }

        public int GetDataCount()
        {
            return _dataList.Count;
        }

        #endregion


        public List<Data> GetDataList()
        {
            return _dataList;
        }

        public Dictionary<int, Item> GetItemDict()
        {
            return _itemDict;
        }

        /// <summary>
        /// 获取某个item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public Item GetItem(int idx)
        {
            if (_itemDict.TryGetValue(idx, out Item item))
            {
                return item;
            }

            return null;
        }

        public T GetStartItem<T>() where T : LoopScrollItem
        {
            if (_itemDict.TryGetValue(_scrollerPro.StartDataIndex, out var item))
            {
                return item as T;
            }

            return null;
        }

        public T GetItemByScrollPosition<T>(float scrollPosition) where T : LoopScrollItem
        {
            var index = _scrollerPro.GetCellViewIndexAtPosition(scrollPosition);
            if (_itemDict.TryGetValue(index, out var item))
            {
                return item as T;
            }

            return null;
        }

        public T GetEndItem<T>() where T : LoopScrollItem
        {
            if (_itemDict.TryGetValue(_scrollerPro.EndDataIndex, out var item))
            {
                return item as T;
            }

            return null;
        }

        public void SetClickFunc(Action<object> func)
        {
            this._clickFunc = func;
        }


        /// <summary>
        /// 设置选择某个item
        /// </summary>
        /// <param name="index"></param>
        public void SetSelect(int index)
        {
            if (_itemDict.Count == 0 || _dataList.Count == 0)
                return;
            CurSelect = index;
            if (CurSelect >= _dataList.Count)
            {
                CurSelect = 0;
            }

            if (_needSelectWithClick)
            {
                if (_selectAction != null && CurSelect >= 0)
                {
                    _selectAction(CurSelect, _dataList[CurSelect]);
                }

                if (_itemDict.Count > 0)
                {
                    foreach (var item in _itemDict.Values)
                    {
                        item.OnItemSelect(CurSelect);
                    }
                }
            }

            if (_clickFunc != null)
            {
                _clickFunc(_dataList[CurSelect]);
            }
        }

        #region 设置大小

        public void SetSize(int index, Vector2 size)
        {
            if (_sizeDict.ContainsKey(index))
            {
                _sizeDict[index] = size;
            }

            var groupIndex = index / _scrollerPro.NumberOfCellsPerRow;

            var cellPosition = _scrollerPro.Scroller.GetScrollPositionForCellViewIndex(groupIndex, CellViewPositionEnum.Before);
            var tweenCellOffset = cellPosition - _scrollerPro.Scroller.ScrollPosition;
            _scrollerPro.IgnoreLoopJump(true);
            _scrollerPro.Scroller.ReloadData();
            cellPosition = _scrollerPro.Scroller.GetScrollPositionForCellViewIndex(groupIndex, CellViewPositionEnum.Before);
            _scrollerPro.Scroller.SetScrollPositionImmediately(cellPosition - tweenCellOffset);
            _scrollerPro.IgnoreLoopJump(false);
        }

        public void SetSizeX(int index, int x)
        {
            if (_sizeDict.ContainsKey(index))
            {
                var size = _sizeDict[index];
                size.x = x;
                SetSize(index, size);
            }
        }

        public void SetSizeY(int index, int y)
        {
            if (_sizeDict.ContainsKey(index))
            {
                var size = _sizeDict[index];
                size.y = y;
                SetSize(index, size);
            }
        }

        #endregion

        #region 预加载大小

        public void PreloadSize(int index, Vector2 size)
        {
            if (_sizeDict.ContainsKey(index))
            {
                _sizeDict[index] = size;
            }
            else
            {
                _sizeDict.Add(index, size);
            }
        }

        public void PreloadSizeX(int index, int x)
        {
            if (_sizeDict.ContainsKey(index))
            {
                _sizeDict[index] = new Vector2(x, ItemSize.y);
            }
            else
            {
                _sizeDict.Add(index, new Vector2(x, ItemSize.y));
            }
        }

        public void PreloadSizeY(int index, int y)
        {
            if (_sizeDict.ContainsKey(index))
            {
                _sizeDict[index] = new Vector2(ItemSize.x, y);
            }
            else
            {
                _sizeDict.Add(index, new Vector2(ItemSize.x, y));
            }
        }

        #endregion



        /// <summary>
        /// 清理列表
        /// </summary>
        public void ClearList()
        {
            foreach (var item in _itemDict.Values)
            {
                _pool.Release(item);
                item.gameObject = null;
            }

            _itemDict.Clear();
            _dataList.Clear();
            _sizeDict.Clear();
            _scrollerPro.ClearAll();
        }


        /// <summary>
        /// 填充
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="startItem"></param>
        /// <param name="loopJumpDirectionEnum"></param>
        public void Refill(List<Data> datas, int startItem = -1, LoopJumpDirectionEnum loopJumpDirectionEnum = LoopJumpDirectionEnum.Closest)
        {
            var oldCount = _dataList.Count;

            _dataList.Clear();

            if (datas == null || datas.Count == 0)
            {
                //清理一下上次的item
                if (oldCount > 0)
                {
                    foreach (var item in _itemDict.Values)
                    {
                        _pool.Release(item);
                        item.gameObject = null;
                    }

                    _itemDict.Clear();
                    _scrollerPro.ClearAll();
                }

                return;
            }

            _dataList.AddRange(datas);

            for (int i = 0; i < datas.Count; i++)
            {
                if (!_sizeDict.ContainsKey(i))
                {
                    _sizeDict.Add(i, ItemSize);
                }
            }

            if (oldCount == datas.Count)
            {
                var pos = _scrollerPro.ScrollPosition;
                _scrollerPro.RefreshActiveCellViews();
                _scrollerPro.ScrollPosition = pos;
            }
            else if (oldCount > 0)
            {
                foreach (var item in _itemDict.Values)
                {
                    _pool.Release(item);
                    item.gameObject = null;
                }

                _itemDict.Clear();
                _scrollerPro.ClearAll();
                _scrollerPro.ReloadData();
            }
            else
            {
                _scrollerPro.ReloadData();
            }

            if (startItem >= 0)
            {
                JumpToDataIndex(startItem, loopJumpDirectionEnum: loopJumpDirectionEnum);
            }
        }

        /// <summary>
        /// 追加数据
        /// </summary>
        /// <param name="datas"></param>
        public void Append(List<Data> datas)
        {
            _dataList.AddRange(datas);

            for (int i = 0; i < _dataList.Count; i++)
            {
                if (!_sizeDict.ContainsKey(i))
                {
                    _sizeDict.Add(i, ItemSize);
                }
            }

            var pos = _scrollerPro.ScrollPosition;
            _scrollerPro.ReloadData();
            _scrollerPro.ScrollPosition = pos;
        }

        /// <summary>
        /// 跳转
        /// </summary>
        /// <param name="startItem"></param>
        /// <param name="jumpComplete"></param>
        /// <param name="tweenType"></param>
        /// <param name="tweenTime"></param>
        /// <param name="loopJumpDirectionEnum"></param>
        public void JumpToDataIndex(int startItem, Action jumpComplete = null, EnhancedScroller.TweenType tweenType = EnhancedScroller.TweenType.immediate, float tweenTime = 0, LoopJumpDirectionEnum loopJumpDirectionEnum = LoopJumpDirectionEnum.Closest, float scrollerOffset = 0, float cellOffset = 0)
        {
            if (startItem >= 0)
            {
                void Complete()
                {
                    jumpComplete?.Invoke();
                }

                _scrollerPro.JumpToDataIndex(startItem / _scrollerPro.NumberOfCellsPerRow, jumpComplete: Complete, tweenType: tweenType, tweenTime: tweenTime, loopJumpDirection: loopJumpDirectionEnum, scrollerOffset: scrollerOffset, cellOffset: cellOffset);
            }
        }

        /// <summary>
        /// 设置可以不可以滑动
        /// </summary>
        /// <param name="enable"></param>
        public void SetScroll(bool enable)
        {
            _scrollerPro.SetScroll(enable);
        }
    }
}