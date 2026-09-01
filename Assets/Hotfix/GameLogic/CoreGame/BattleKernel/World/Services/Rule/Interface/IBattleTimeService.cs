namespace LccHotfix
{
    public interface IBattleTimeService
    {
        float DeltaTime { get; }
        long NowTicks { get; }
    }
}
