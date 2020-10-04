namespace Model
{
    public class Manager : Singleton<Manager>
    {
        public void InitManager()
        {
            UIEventManager.Instance.Publish(UIEventType.Launch);
        }
    }
}