using System.Collections.Generic;

namespace LccHotfix
{
    public partial class LogicConfigs_DemoSubobject : LogicConfigBase
    {
        public LogicConfigs_DemoSubobject(string name) : base(name, 8)
        {
            DefaultLogicType = typeof(SubobjectLogic);
            InitConfigs();
        }

        private void InitConfigs()
        {
            AddConfig(DemoBattleConfigIds.ProjectileSubobjectLogic, new List<ICustomNodeCfg>
            {
                Subobject_InitColliderByPbCfg(),
                Subobject_SetLocomotionStraightDir(5f),
                new HandleSubobjectHitCmdCfg(CvKey.CV_SbjHitFxRes),
            });
        }
    }
}
