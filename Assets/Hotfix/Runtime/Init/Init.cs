namespace LccHotfix
{
    public class Init
    {
        public static void InitHotfix()
        {
            Manager.Instance.InitManager();
            EventManager.Instance.InitManager();
            UIEventManager.Instance.InitManager();

            EventManager.Instance.Publish(new Start());
        }
    }
}