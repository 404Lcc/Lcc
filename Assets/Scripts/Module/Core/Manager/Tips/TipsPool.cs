using UnityEngine;

namespace LccModel
{
    public class TipsPool : APool<Tips>
    {
        public TipsPool()
        {
        }
        public TipsPool(int size) : base(size)
        {
        }
        public override void InitPool()
        {
            for (int i = 0; i < size; i++)
            {
                Tips tips = ObjectBaseFactory.Create<Tips, GameObject>(null, AssetManager.Instance.InstantiateAsset("Tips", false, false, Objects.Canvas.transform, AssetType.Panel, AssetType.Tool));
                Enqueue(tips);
            }
        }
        public override void Enqueue(Tips item)
        {
            item.gameObject.transform.SetParent(Objects.Canvas.transform);
            item.gameObject.SetActive(false);
            poolQueue.Enqueue(item);
        }
        public override Tips Dequeue()
        {
            if (Count == 0)
            {
                InitPool();
            }
            Tips tips = poolQueue.Dequeue();
            tips.gameObject.transform.SetParent(Objects.Canvas.transform);
            tips.gameObject.SetActive(true);
            return tips;
        }
    }
}