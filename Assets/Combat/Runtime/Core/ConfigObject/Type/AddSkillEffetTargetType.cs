using Sirenix.OdinInspector;

namespace LccModel
{
    [LabelText("���ö���")]
    public enum AddSkillEffetTargetType
    {
        [LabelText("����Ŀ��")]
        SkillTarget = 0,
        [LabelText("����")]
        Self = 1,
        [LabelText("����")]
        Other = 2,
    }
}