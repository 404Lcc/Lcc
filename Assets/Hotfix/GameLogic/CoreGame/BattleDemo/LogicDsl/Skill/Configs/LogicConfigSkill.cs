using System.Collections.Generic;

namespace LccHotfix
{
    public partial class LogicConfigs_Skill : LogicConfigBase
    {
        public LogicConfigs_Skill(string name) : base(name, 8)
        {
            DefaultLogicType = typeof(SkillLogic);
            InitConfigs();
        }

        private void InitConfigs()
        {
            AddConfig(DemoBattleConfigIds.ProjectileSkillLogic, new List<ICustomNodeCfg>
            {
                Seq(
                    Delay(0.15f),
                    FighterSpawnSubobjectTo(CvKey.CV_SpawnSbjTid, CvKey.CV_TargetPos, null),
                    Delay(0.05f)
                ),
            });
        }
    }
}
