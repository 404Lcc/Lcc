namespace LccModel
{
    public class Singleton<T> : AObjectBase where T : AObjectBase
    {
        private static readonly object _lockObject = new object();
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        _instance = GameEntity.Instance.AddChildren<T>();
                    }
                }
                return _instance;
            }
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            _instance = null;
        }
    }
}