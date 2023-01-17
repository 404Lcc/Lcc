using Sirenix.OdinInspector;

namespace LccModel
{
    [LabelText("作用对象")]
    public enum AddSkillEffetTargetType
    {
        [LabelText("技能目标")]
        SkillTarget = 0,
        [LabelText("自身")]
        Self = 1,
        [LabelText("其他")]
        Other = 2,
    }
}