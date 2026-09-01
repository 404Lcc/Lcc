using PBConfig;

namespace LccHotfix
{
    public interface IHasFighter
    {
        TFighter FighterCfg { get; }
        int BattleUnitTid { get; }
    }
}
