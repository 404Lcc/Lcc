using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class LoopScrollItemPool<T> : APool<T> where T : LoopScrollItem
    {
        public AObjectBase parent;
        public GameObject itemPrefab;
        public Transform content;
        public LoopScrollItemPool(AObjectBase parent, int maxSize, GameObject itemPrefab, Transform content) : base(5, maxSize)
        {
            this.parent = parent;
            this.itemPrefab = itemPrefab;
            this.content = content;
        }
        protected override T Create()
        {
            GameObject obj = GameObject.Instantiate(itemPrefab, content, false);
            RectTransform rect = obj.transform as RectTransform;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            T item = parent.AddChildren<T, GameObject>(obj);
            return item;
        }
        protected override void Get(T item)
        {
            item.gameObject.SetActive(true);
        }
        protected override void Release(T item)
        {
            RectTransform rect = item.gameObject.transform as RectTransform;
            item.gameObject.SetActive(false);
            rect.SetParent(content);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
        protected override void Destroy(T item)
        {
            item.Dispose();
        }
    }
}