using PBConfig;

namespace LccHotfix
{
    public interface ISkillLogicOverrideProvider
    {
        int ResolveSkillLogicId(IBattlePlayerInfo playerInfo, TFighter fighterCfg, int defaultLogicId);
    }
}
