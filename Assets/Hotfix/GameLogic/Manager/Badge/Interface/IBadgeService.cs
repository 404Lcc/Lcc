namespace LccHotfix
{
    public interface IBadgeService : IService
    {
        void RegisterHandler(BadgeHandler handler);
        void UnRegisterHandler(BadgeHandler handler);
    }
}