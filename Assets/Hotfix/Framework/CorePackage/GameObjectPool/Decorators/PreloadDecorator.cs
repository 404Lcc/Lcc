namespace LccHotfix
{
    public class PreloadDecorator : GameObjectPoolDecorator
    {
        private int _preloaded = 0;

        public PreloadDecorator(IGameObjectPool pool) : base(pool)
        {

        }

        public override void Update()
        {
            if (_preloaded < PoolSetting.preloadCount)
            {
                for (var i = 0; i < PoolSetting.preloadPerFrame && _preloaded < PoolSetting.preloadCount; i++)
                {
                    Release(ForceSpawm());
                    _preloaded++;
                }
            }
        }
    }
}