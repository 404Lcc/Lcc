using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class LoopScrollItemPool<T> : APool<T> where T : LoopScrollItem
    {
        public AObjectBase parent;
        public LoopScrollItemPool(AObjectBase parent, int maxSize) : base(5, maxSize)
        {
            this.parent = parent;
        }
        protected override T Create()
        {
            T item = parent.AddChildren<T>();
            return item;
        }
        protected override void Get(T item)
        {
        }
        protected override void Release(T item)
        {
        }
        protected override void Destroy(T item)
        {
            item.Dispose();
        }
    }
}