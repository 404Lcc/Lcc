namespace LccHotfix
{
    public class EventManager : AObjectBase
    {
        public static EventManager Instance { get; set; }

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
        }

        public void Publish<T>(T data)
        {
            EventSystem.Instance.Publish(data);
        }
    }
}