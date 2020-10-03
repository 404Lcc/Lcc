namespace Model
{
    public class Manager : Singleton<Manager>
    {
        /// <summary>
        /// 初始化管理类
        /// </summary>
        public void InitManager()
        {
            UIEventManager.Instance.Publish(UIEventType.Launch);
        }
    }
}