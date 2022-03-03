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
                GameObject gameObject = AssetManager.Instance.InstantiateAsset("Tips", false, false, Objects.Canvas.transform, AssetType.Tool);
                GameObjectEntity GameObjectEntity = TipsManager.Instance.AddChildren<GameObjectEntity, GameObject>(gameObject);
                Tips tips = GameObjectEntity.AddComponent<Tips>();
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
            tips.gameObject.SetActive(true);
            return tips;
        }
    }
}