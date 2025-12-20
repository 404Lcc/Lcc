namespace LccHotfix
{
    public interface IFxService : IService
    {
        public FxCache LoadFxCache(EFxOneType fxType, string path, int cost, int capacity, int maxCount, bool isAsyncLoad);
        public FxOne RequestFx_And_Play(EFxOneType fxType, string path, float during = -1, int maxCount = 0, int cost = 0, int costLimitLevel = 0, bool isAsyncLoad = true);
        public FxOne RequestFx_With_Cost(EFxOneType fxType, string path, int cost, int maxCount, int costLimitLevel, bool isAsyncLoad);
    }
}