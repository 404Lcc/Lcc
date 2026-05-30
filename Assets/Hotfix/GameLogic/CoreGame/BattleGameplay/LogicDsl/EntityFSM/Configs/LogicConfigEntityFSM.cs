using System.Collections.Generic;

namespace LccHotfix
{
    public partial class LogicConfigs_EntityFSM : LogicConfigBase
    {
        public LogicConfigs_EntityFSM(string name) : base(name, 8)
        {
            DefaultLogicType = typeof(BattleFSM);
            InitConfigs();
        }

        private void InitConfigs()
        {
            AddConfig(DemoBattleConfigIds.UnitFsmLogic, new List<ICustomNodeCfg>
            {
                FSM("EST_AutoCast", new List<ICustomNodeCfg>
                {
                    CustomState("EST_AutoCast", Bhv<DemoAutoCastSkillBhv>()),
                }),
            });
        }
    }
}
