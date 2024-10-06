using UnityEngine;
using EnhancedUI.EnhancedScroller;
using System;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace LccModel
{
    //垂直列表对其方式  左中右对其
    public enum PivotHorizontal
    {
        [LabelText("左对齐")]
        Left,
        [LabelText("单个item居中对齐")]
        Center,//根据每组固定item数量居中
        [LabelText("居中对齐")]
        Middle,//根据真实的item数量居中
        [LabelText("右对齐")]
        Right,
    }
    //水平列表对其方式  上中下对其
    public enum PivotVerticle
    {
        [LabelText("上对齐")]
        Top,
        [LabelText("单个item居中对齐")]
        Center,
        [LabelText("居中对齐")]
        Middle,
        [LabelText("下对齐")]
        Bottom,
    }
    public class ScrollerPro : EnhancedScroller, IEnhancedScrollerDelegate
    {
        public Action<GroupBase, int, int> GetObjectHandler;
        public Action<int> ReturnObjectHandler;
        public Action<int> ProvideDataHandler;
        public Func<int, int> GetGroupSizeHandler;
        public Func<int> GetDataCountHandler;

        //垂直列表对其方式  左中右对其
        [ShowIf("@this.scrollDirection == ScrollDirectionEnum.Vertical")]
        public PivotHorizontal pivotHorizontal;
        //水平列表对其方式  上中下对其
        [ShowIf("@this.scrollDirection == ScrollDirectionEnum.Horizontal")]
        public PivotVerticle pivotVertical;
        [LabelText("是否需要滚动")]
        public bool needScroller = true;

        public GroupBase groupPrefab;

        public bool isGrid = false;
        public EnhancedScroller Scroller => this;

        //NumberOfCellsPerRow一排or一行有几个
        private int NumberOfCellsPerRow => groupPrefab.gridCount;

        public void Awake()
        {
            Delegate = this;
            cellViewVisibilityChanged = CellViewVisibilityChanged;
            cellViewInstantiated = CellViewInstantiated;


            SetScroll(needScroller);
        }

        /// <summary>
        /// 设置可以不可以滑动
        /// </summary>
        /// <param name="enable"></param>
        public void SetScroll(bool enable)
        {
            this.needScroller = enable;
            if (enable)
            {
                GetComponent<ScrollRect>().horizontal = scrollDirection == ScrollDirectionEnum.Horizontal;
                GetComponent<ScrollRect>().vertical = scrollDirection == ScrollDirectionEnum.Vertical;
            }
            else
            {
                GetComponent<ScrollRect>().horizontal = false;
                GetComponent<ScrollRect>().vertical = false;
            }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            GroupBase group = scroller.GetCellView(groupPrefab) as GroupBase;

            group.name = "Group " + (dataIndex * NumberOfCellsPerRow).ToString() + " to " + ((dataIndex * NumberOfCellsPerRow) + NumberOfCellsPerRow - 1).ToString();
            group.SetGroup(dataIndex / NumberOfCellsPerRow, dataIndex * NumberOfCellsPerRow, ((dataIndex * NumberOfCellsPerRow) + NumberOfCellsPerRow - 1));

            return group;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (GetGroupSizeHandler == null) return 0;
            return GetGroupSizeHandler(dataIndex);
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (GetDataCountHandler == null) return 0;
            return Mathf.CeilToInt((float)GetDataCountHandler() / (float)NumberOfCellsPerRow);
        }

        public void CellViewVisibilityChanged(EnhancedScrollerCellView cellView)
        {
            GroupBase group = cellView as GroupBase;

            group.RefreshCellView();
        }
        public void CellViewInstantiated(EnhancedScroller scroller, EnhancedScrollerCellView cellView)
        {
            cellView.gameObject.SetActive(true);
        }
    }
}