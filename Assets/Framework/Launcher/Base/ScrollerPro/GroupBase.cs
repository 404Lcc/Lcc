using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;
using static EnhancedUI.EnhancedScroller.EnhancedScroller;
using UnityEngine.UIElements;

namespace LccModel
{
    public class GroupBase : EnhancedScrollerCellView
    {
        public int groupIndex;
        public int groupStart;

        public int gridCount;
        public ScrollerPro scrollerPro;
        //public List<Transform> transformList = new List<Transform>();

        public void InitGroup(ScrollerPro scrollerPro, Transform itemPrefab)
        {
            this.scrollerPro = scrollerPro;
            RectTransform rect = transform as RectTransform;
            if (scrollerPro.isGrid)
            {
                RectTransform itemRect = itemPrefab as RectTransform;
                gridCount = (int)(rect.sizeDelta().x / (itemRect.sizeDelta().x + scrollerPro.Scroller.spacing));

                //for (int i = 0; i < gridCount; i++)
                //{
                //    var item = Instantiate(itemPrefab, transform).transform;
                //    item.gameObject.SetActive(false);
                //    transformList.Add(item);
                //}
            }
            else if (!scrollerPro.isGrid)
            {
                gridCount = 1;
                //var item = Instantiate(itemPrefab, transform).transform;
                //item.gameObject.SetActive(false);
                //transformList.Add(item);
            }
            cellIdentifier = "GroupBase";
        }



        public void SetGroup(int groupIndex, int groupStart)
        {
            this.groupIndex = groupIndex;
            this.groupStart = groupStart;
        }

        public override void RefreshCellView()
        {
            int max = scrollerPro.GetDataCountHandler();
            for (int i = 0; i < gridCount; i++)
            {
                if (groupStart + i >= max) continue;
                if (active)
                {
                    scrollerPro.GetObjectHandler?.Invoke(this, groupStart + i);
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