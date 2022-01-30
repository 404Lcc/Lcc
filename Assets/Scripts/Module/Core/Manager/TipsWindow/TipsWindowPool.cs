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
                GameObject gameObject = AssetManager.Instance.InstantiateAsset("TipsWindow", false, false, Objects.Canvas.transform, AssetType.Panel, AssetType.Tool);
                GameObjectComponent gameObjectComponent = ObjectBaseFactory.Create<GameObjectComponent, GameObject>(null, gameObject);
                TipsWindow tipsWindow = gameObjectComponent.AddComponent<TipsWindow>();
                Enqueue(tipsWindow);
            }
        }
        public override void Enqueue(TipsWindow item)
        {
            item.gameObject.transform.SetParent(Objects.Canvas.transform);
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
            tipsWindow.gameObject.transform.SetParent(Objects.Canvas.transform);
            tipsWindow.gameObject.SetActive(true);
            return tipsWindow;
        }
    }
}