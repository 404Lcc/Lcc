namespace Model
{
    public class TipsPool
    {
        public Pool<Tips> tipspool;
        public int tipsmax;
        public TipsPool()
        {
        }
        public TipsPool(int tipsmax)
        {
            this.tipsmax = tipsmax;
            tipspool = new Pool<Tips>(tipsmax);
        }
        public void InitPool()
        {
            for (int i = 0; i < tipsmax; i++)
            {
                Tips tips = GameUtil.GetComponent<Tips>(IO.assetManager.LoadGameObject("Tips", false, false, IO.gui.transform, AssetType.UI, AssetType.Tool));
                Enqueue(tips);
            }
        }
        public int Count
        {
            get
            {
                return tipspool.Count;
            }
        }
        public void Enqueue(Tips tips)
        {
            tips.transform.SetParent(IO.gui.transform);
            tips.gameObject.SetActive(false);
            tipspool.Enqueue(tips);
        }
        public Tips Dequeue()
        {
            if (Count == 0)
            {
                InitPool();
            }
            Tips tips = tipspool.Dequeue();
            tips.transform.SetParent(IO.gui.transform);
            tips.gameObject.SetActive(true);
            return tips;
        }
    }
}