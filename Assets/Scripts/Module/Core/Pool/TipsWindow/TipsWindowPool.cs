namespace Model
{
    public class TipsWindowPool : APool<TipsWindow>
    {
        public TipsWindowPool(int size) : base(size)
        {
        }
        public override void InitPool()
        {
            for (int i = 0; i < size; i++)
            {
                TipsWindow tipsWindow = LccViewFactory.CreateView<TipsWindow>(AssetManager.Instance.LoadGameObject("TipsWindow", false, false, Objects.GUI.transform, AssetType.UI, AssetType.Tool));
                Enqueue(tipsWindow);
            }
        }
        public override void Enqueue(TipsWindow item)
        {
            item.transform.SetParent(Objects.GUI.transform);
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
            tipsWindow.transform.SetParent(Objects.GUI.transform);
            tipsWindow.gameObject.SetActive(true);
            return tipsWindow;
        }
    }
}