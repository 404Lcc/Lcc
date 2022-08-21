namespace LccHotfix
{
    public class Singleton<T> : AObjectBase where T : AObjectBase
    {
        public static T Instance
        {
            get
            {
                if (GameEntity.Instance.Components.TryGetValue(typeof(T), out AObjectBase instance))
                {
                    return (T)instance;
                }
                else
                {
                    return GameEntity.Instance.AddComponent<T>();
                }
            }
        }
    }
}