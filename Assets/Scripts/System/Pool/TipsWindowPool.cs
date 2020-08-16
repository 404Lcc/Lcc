namespace Model
{
    public class TipsWindowPool
    {
        public Pool<TipsWindow> tipswindowpool;
        public int tipswindowmax;
        public TipsWindowPool()
        {
        }
        public TipsWindowPool(int tipswindowmax)
        {
            this.tipswindowmax = tipswindowmax;
            tipswindowpool = new Pool<TipsWindow>(tipswindowmax);
        }
        public void InitPool()
        {
            for (int i = 0; i < tipswindowmax; i++)
            {
                TipsWindow tipswindow = GameUtil.GetComponent<TipsWindow>(IO.assetManager.LoadGameObject("TipsWindow", false, false, IO.gui.transform, AssetType.UI, AssetType.Tool));
                Enqueue(tipswindow);
            }
        }
        public int Count
        {
            get
            {
                return tipswindowpool.Count;
            }
        }
        public void Enqueue(TipsWindow tipswindow)
        {
            tipswindow.transform.SetParent(IO.gui.transform);
            tipswindow.gameObject.SetActive(false);
            tipswindowpool.Enqueue(tipswindow);
        }
        public TipsWindow Dequeue()
        {
            if (Count == 0)
            {
                InitPool();
            }
            TipsWindow tipswindow = tipswindowpool.Dequeue();
            tipswindow.transform.SetParent(IO.gui.transform);
            tipswindow.gameObject.SetActive(true);
            return tipswindow;
        }
    }
}