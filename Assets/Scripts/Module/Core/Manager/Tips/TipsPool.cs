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
        public override async void InitPool()
        {
            for (int i = 0; i < size; i++)
            {
                Tips tips = LccViewFactory.CreateView<Tips>(await AssetManager.Instance.InstantiateAssetAsync("Tips", false, false, Objects.Canvas.transform, AssetType.Panel, AssetType.Tool));
                Enqueue(tips);
            }
        }
        public override void Enqueue(Tips item)
        {
            item.transform.SetParent(Objects.Canvas.transform);
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
            tips.transform.SetParent(Objects.Canvas.transform);
            tips.gameObject.SetActive(true);
            return tips;
        }
    }
}