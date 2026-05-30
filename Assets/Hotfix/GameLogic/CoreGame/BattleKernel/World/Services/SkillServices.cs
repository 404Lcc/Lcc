using PBConfig;

namespace LccHotfix
{
    public interface ISkillLogicOverrideProvider
    {
        int ResolveSkillLogicId(IBattlePlayerInfo playerInfo, TFighter fighterCfg, int defaultLogicId);
    }

    public partial class LogicWorld
    {
        public ISkillLogicOverrideProvider SkillLogicOverrideProvider { get; private set; }

        public void SetSkillLogicOverrideProvider(ISkillLogicOverrideProvider provider)
        {
            SkillLogicOverrideProvider = provider;
        }
    }
}
