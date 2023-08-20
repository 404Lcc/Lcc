using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;

namespace LccModel
{
    public class GroupBase : EnhancedScrollerCellView
    {
        public int startIndex;
        public int gridCount;
        public ScrollerPro scrollerPro;
        public List<Transform> transformList = new List<Transform>();

        public void InitGroup(ScrollerPro scrollerPro, Transform itemPrefab)
        {
            this.scrollerPro = scrollerPro;
            RectTransform rect = transform as RectTransform;
            if (scrollerPro.isGrid)
            {
                RectTransform itemRect = itemPrefab as RectTransform;
                gridCount = (int)(rect.sizeDelta().x / (itemRect.sizeDelta().x + scrollerPro.Scroller.spacing));

                for (int i = 0; i < gridCount; i++)
                {
                    var item = Instantiate(itemPrefab, transform).transform;
                    item.gameObject.SetActive(false);
                    transformList.Add(item);
                }
            }
            else if (!scrollerPro.isGrid)
            {
                gridCount = 1;
                var item = Instantiate(itemPrefab, transform).transform;
                item.gameObject.SetActive(false);
                transformList.Add(item);
            }
            cellIdentifier = "GroupBase";
        }



        public void SetData(int startIndex)
        {
            this.startIndex = startIndex;
        }
        public void SetSize(Vector2 sizeDelta)
        {
            LayoutElement layoutElement = GetComponent<LayoutElement>();
            layoutElement.minWidth = sizeDelta.x;
            layoutElement.minHeight = sizeDelta.y;
        }

        public override void RefreshCellView()
        {
            int max = scrollerPro.GetDataCountHandler();
            for (int i = 0; i < transformList.Count; i++)
            {
                if (startIndex + i >= max) continue;
                if (active)
                {
                    transformList[i].gameObject.SetActive(true);
                    scrollerPro.GetObjectHandler?.Invoke(transformList[i].transform, startIndex + i);
                    scrollerPro.ProvideDataHandler?.Invoke(transformList[i].transform, startIndex + i);
                }
                else
                {
                    transformList[i].gameObject.SetActive(false);
                    scrollerPro.ReturnObjectHandler?.Invoke(transformList[i].transform, startIndex + i);
                }
            }
        }
    }
}