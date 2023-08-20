using UnityEngine;
using EnhancedUI.EnhancedScroller;
using System;

namespace LccModel
{
    public class ScrollerPro : EnhancedScroller, IEnhancedScrollerDelegate
    {
        public Action<Transform, int> GetObjectHandler;
        public Action<Transform, int> ReturnObjectHandler;
        public Action<Transform, int> ProvideDataHandler;
        public Func<int> GetDataCountHandler;


        public GroupBase groupPrefab;

        private float _groupSize = -1;
        public float GroupSize
        {
            get
            {
                if (_groupSize == -1)
                {
                    RectTransform rect = groupPrefab.transform as RectTransform;
                    _groupSize = Scroller.scrollDirection == ScrollDirectionEnum.Vertical ? rect.sizeDelta().y : rect.sizeDelta().x;
                }
                return _groupSize;
            }
        }

        private int _pageCount = -1;

        public int PageCount
        {
            get
            {
                if (_pageCount == -1)
                {
                    _pageCount = Scroller.scrollDirection == ScrollDirectionEnum.Vertical ? (int)Scroller.ScrollRectSize / (int)GroupSize : (int)Scroller.ScrollRectSize / (int)GroupSize;
                }
                return _pageCount;
            }
        }


        public bool isGrid = false;

        private int _numberOfCellsPerRow = -1;
        private int NumberOfCellsPerRow
        {
            get
            {
                if (_numberOfCellsPerRow == -1)
                {
                    _numberOfCellsPerRow = groupPrefab.gridCount;
                }
                return _numberOfCellsPerRow;
            }
        }


        public EnhancedScroller Scroller => this;

        public void Start()
        {

            Scroller.Delegate = this;
            Scroller.cellViewVisibilityChanged = CellViewVisibilityChanged;
            Scroller.cellViewInstantiated = CellViewInstantiated;
        }

        public void RefershData()
        {
            Scroller.RefreshActiveCellViews();
        }
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            GroupBase group = scroller.GetCellView(groupPrefab) as GroupBase;

            group.name = "Group " + (dataIndex * NumberOfCellsPerRow).ToString() + " to " + ((dataIndex * NumberOfCellsPerRow) + NumberOfCellsPerRow - 1).ToString();
            group.SetData(dataIndex * NumberOfCellsPerRow);

            return group;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return GroupSize;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (isGrid == false)
            {
                return GetDataCountHandler();
            }
            else
            {
                return Mathf.CeilToInt((float)GetDataCountHandler() / (float)NumberOfCellsPerRow);
            }
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