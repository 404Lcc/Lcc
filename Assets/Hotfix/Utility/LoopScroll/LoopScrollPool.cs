using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class LoopScrollPool
    {
        public ScrollerPro scrollerPro;
        public ILoopScroll loopScroll;
        public GameObject itemPrefab;

        public LoopScrollPool(ScrollerPro scrollerPro, ILoopScroll loopScroll, GameObject itemPrefab)
        {
            this.scrollerPro = scrollerPro;
            this.loopScroll = loopScroll;
            this.itemPrefab = itemPrefab;
        }

        public T Get<T>() where T : LoopScrollItem, new()
        {
            var item = ReferencePool.Acquire<T>();

            if (item.gameObject == null)
            {
                GameObject obj = GameObject.Instantiate(itemPrefab, scrollerPro.transform, false);
                RectTransform objRect = obj.transform as RectTransform;
                objRect.localPosition = Vector3.zero;
                objRect.localRotation = Quaternion.identity;
                objRect.localScale = Vector3.one;
                item.Init(loopScroll, obj);
            }
            item.gameObject.SetActive(true);



            return item;
        }
        public void Release<T>(T item) where T : LoopScrollItem, new()
        {
            ReferencePool.Release(item);
        }
        public void Clear<T>() where T : LoopScrollItem, new()
        {
            ReferencePool.RemoveAll<T>();
        }
    }
}