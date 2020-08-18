namespace Model
{
    public class TipsPool
    {
        public Pool<Tips> tipsPool;
        public int tipsMax;
        public TipsPool()
        {
        }
        public TipsPool(int tipsMax)
        {
            this.tipsMax = tipsMax;
            tipsPool = new Pool<Tips>(tipsMax);
        }
        public void InitPool()
        {
            for (int i = 0; i < tipsMax; i++)
            {
                Tips tips = GameUtil.GetComponent<Tips>(IO.assetManager.LoadGameObject("Tips", false, false, IO.gui.transform, AssetType.UI, AssetType.Tool));
                Enqueue(tips);
            }
        }
        public int Count
        {
            get
            {
                return tipsPool.Count;
            }
        }
        public void Enqueue(Tips tips)
        {
            tips.transform.SetParent(IO.gui.transform);
            tips.gameObject.SetActive(false);
            tipsPool.Enqueue(tips);
        }
        public Tips Dequeue()
        {
            if (Count == 0)
            {
                InitPool();
            }
            Tips tips = tipsPool.Dequeue();
            tips.transform.SetParent(IO.gui.transform);
            tips.gameObject.SetActive(true);
            return tips;
        }
    }
}