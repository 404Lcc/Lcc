using UnityEngine;

namespace LccModel
{
    public class TipsWindowPool : APool<TipsWindow>
    {
        public TipsWindowPool()
        {
        }
        public TipsWindowPool(int size) : base(size)
        {
        }
        public override void InitPool()
        {
            for (int i = 0; i < size; i++)
            {
                GameObject gameObject = AssetManager.Instance.InstantiateAsset("TipsWindow", GlobalManager.Instance.PoolRoot, AssetType.Tool);
                GameObjectEntity GameObjectEntity = TipsWindowManager.Instance.AddChildren<GameObjectEntity, GameObject>(gameObject);
                TipsWindow tipsWindow = GameObjectEntity.AddComponent<TipsWindow>();
                Enqueue(tipsWindow);
            }
        }
        public override void Enqueue(TipsWindow item)
        {
            item.gameObject.transform.SetParent(GlobalManager.Instance.PoolRoot);
            item.gameObject.SetActive(false);
            poolQueue.Enqueue(item);
        }
        public override TipsWindow Dequeue()
        {
            if (Count == 0)
            {
                InitPool();
            }
            TipsWindow tipsWindow = poolQueue.Dequeue();
            tipsWindow.gameObject.SetActive(true);
            return tipsWindow;
        }
    }
}