namespace LccHotfix
{
    public interface ISubobjectModelOverrideProvider
    {
        string ResolveMainModelPath(IBattlePlayerInfo playerInfo, uint subobjectTid, string defaultPath);
    }

    public partial class LogicWorld
    {
        public ISubobjectModelOverrideProvider SubobjectModelOverrideProvider { get; private set; }

        public void SetSubobjectModelOverrideProvider(ISubobjectModelOverrideProvider provider)
        {
            SubobjectModelOverrideProvider = provider;
        }
    }
}
