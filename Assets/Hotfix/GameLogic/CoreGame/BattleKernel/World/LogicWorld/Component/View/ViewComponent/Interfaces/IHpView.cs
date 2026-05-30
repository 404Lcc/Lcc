namespace LccHotfix
{
    public interface IHpView : IViewWrapper
    {
        void SetHp(long entityId, double hp, double maxHp);
    }
}
