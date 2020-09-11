namespace Model
{
    public class Manager : Singleton<Manager>
    {
        public override void Start()
        {
            //TipsManager.Instance.InitManager(new TipsPool(1));
            //TipsWindowManager.Instance.InitManager(new TipsWindowPool(1));
            //开屏界面-资源更新界面-初始化IL-开始界面
            PanelManager.Instance.OpenPanel(PanelType.Launch);
        }
        /// <summary>
        /// 初始化管理类
        /// </summary>
        public void InitManagers()
        {
        }
    }
}