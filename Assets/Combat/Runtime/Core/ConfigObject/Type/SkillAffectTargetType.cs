using Sirenix.OdinInspector;

namespace LccModel
{
    [LabelText("�������ö���")]
    public enum SkillAffectTargetType
    {
        [LabelText("����")]
        Self = 0,
        [LabelText("����")]
        SelfTeam = 1,
        [LabelText("�з�")]
        EnemyTeam = 2,
    }
}