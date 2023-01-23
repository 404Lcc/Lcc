using Sirenix.OdinInspector;

namespace LccModel
{
    [LabelText("技能作用对象")]
    public enum SkillAffectTargetType
    {
        [LabelText("自身")]
        Self = 0,
        [LabelText("己方")]
        SelfTeam = 1,
        [LabelText("敌方")]
        EnemyTeam = 2,
    }
}