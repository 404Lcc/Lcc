using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class LoopScrollItemPool<T> : APool<T> where T : LoopScrollItem
    {
        public AObjectBase parent;
        public GameObject gameObject;
        public Transform content;
        public LoopScrollItemPool(AObjectBase parent, int maxSize, GameObject gameObject, Transform content) : base(5, maxSize)
        {
            this.parent = parent;
            this.gameObject = gameObject;
            this.content = content;
        }
        protected override T Create()
        {
            GameObject go = GameObject.Instantiate(gameObject);
            T item = parent.AddChildren<T, GameObject>(go);
            return item;
        }
        protected override void Get(T item)
        {
            RectTransform rect = (RectTransform)item.gameObject.transform;
            item.gameObject.SetActive(true);
            rect.SetParent(content);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
        protected override void Release(T item)
        {
            RectTransform rect = (RectTransform)item.gameObject.transform;
            item.gameObject.SetActive(false);
            rect.SetParent(content.parent);
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