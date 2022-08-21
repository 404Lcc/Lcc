namespace LccModel
{
    public class GameEntity : AObjectBase
    {
        private static GameEntity _instance;
        public static GameEntity Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Create<GameEntity>();
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