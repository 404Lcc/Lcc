using UnityEngine;
using EnhancedUI.EnhancedScroller;
using static EnhancedUI.EnhancedScroller.EnhancedScroller;

namespace LccModel
{
    public class GroupBase : EnhancedScrollerCellView
    {
        public int groupIndex;
        public int groupStart;
        public int groupEnd;

        public int gridCount;

        public Vector3 groupSize;

        public ScrollerPro scrollerPro;

        public void InitGroup(ScrollerPro scrollerPro, Transform itemPrefab)
        {
            this.scrollerPro = scrollerPro;
            RectTransform rect = transform as RectTransform;
            groupSize = rect.SizeDelta();
            if (scrollerPro.isGrid)
            {
                RectTransform itemRect = itemPrefab as RectTransform;

                if (scrollerPro.scrollDirection == ScrollDirectionEnum.Vertical)
                {
                    gridCount = (int)(rect.SizeDelta().x / (itemRect.SizeDelta().x + scrollerPro.Scroller.spacing));
                }
                else
                {
                    gridCount = (int)(rect.SizeDelta().y / (itemRect.SizeDelta().y + scrollerPro.Scroller.spacing));
                }
            }
            else if (!scrollerPro.isGrid)
            {
                gridCount = 1;
            }
            cellIdentifier = "GroupBase";
        }



        public void SetGroup(int groupIndex, int groupStart, int groupEnd)
        {
            this.groupIndex = groupIndex;
            this.groupStart = groupStart;
            this.groupEnd = groupEnd;
        }

        public override void RefreshCellView()
        {
            int max = scrollerPro.GetDataCountHandler();
            int length = 0;
            for (int i = 0; i < gridCount; i++)
            {
                if (groupStart + i >= max) continue;
                length++;
            }


            for (int i = 0; i < length; i++)
            {
                if (active)
                {
                    scrollerPro.GetObjectHandler?.Invoke(this, groupStart + i, groupStart + length - 1);
                    scrollerPro.ProvideDataHandler?.Invoke(groupStart + i);
                }
                else
                {
                    scrollerPro.ReturnObjectHandler?.Invoke(groupStart + i);
                }
            }
        }
    }
}