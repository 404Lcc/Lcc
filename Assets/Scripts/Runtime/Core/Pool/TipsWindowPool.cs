namespace Model
{
    public class TipsWindowPool
    {
        public Pool<TipsWindow> tipsWindowPool;
        public int tipsWindowMax;
        public TipsWindowPool()
        {
        }
        public TipsWindowPool(int tipsWindowMax)
        {
            this.tipsWindowMax = tipsWindowMax;
            tipsWindowPool = new Pool<TipsWindow>(tipsWindowMax);
        }
        public void InitPool()
        {
            for (int i = 0; i < tipsWindowMax; i++)
            {
                TipsWindow tipsWindow = GameUtil.GetComponent<TipsWindow>(AssetManager.Instance.LoadGameObject("TipsWindow", false, false, Objects.gui.transform, AssetType.UI, AssetType.Tool));
                Enqueue(tipsWindow);
            }
        }
        public int Count
        {
            get
            {
                return tipsWindowPool.Count;
            }
        }
        public void Enqueue(TipsWindow tipsWindow)
        {
            tipsWindow.transform.SetParent(Objects.gui.transform);
            tipsWindow.gameObject.SetActive(false);
            tipsWindowPool.Enqueue(tipsWindow);
        }
        public TipsWindow Dequeue()
        {
            if (Count == 0)
            {
                InitPool();
            }
            TipsWindow tipsWindow = tipsWindowPool.Dequeue();
            tipsWindow.transform.SetParent(Objects.gui.transform);
            tipsWindow.gameObject.SetActive(true);
            return tipsWindow;
        }
    }
}