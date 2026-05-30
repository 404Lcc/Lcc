namespace LccHotfix
{
    public interface IBattlePlayerInfo
    {
        long PlayerUid { get; }
        int PlayerIndex { get; }
        bool IsLocalPlayer { get; }
    }
}
