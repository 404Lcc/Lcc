namespace LccHotfix
{
    public interface ISubobjectModelOverrideProvider
    {
        string ResolveMainModelPath(IBattlePlayerInfo playerInfo, uint subobjectTid, string defaultPath);
    }
}
