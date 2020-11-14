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
                Tips tips = LccViewFactory.CreateView<Tips>(await AssetManager.Instance.InstantiateAsset("Tips", false, false, Objects.GUI.transform, AssetType.UI, AssetType.Tool));
                Enqueue(tips);
            }
        }
        public override void Enqueue(Tips item)
        {
            item.transform.SetParent(Objects.GUI.transform);
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
            tips.transform.SetParent(Objects.GUI.transform);
            tips.gameObject.SetActive(true);
            return tips;
        }
    }
}