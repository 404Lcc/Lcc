using Sirenix.OdinInspector;

namespace LccModel
{
    [LabelText("Ч��Ӧ��Ŀ��ѡ��ʽ")]
    public enum SkillTargetSelectType
    {
        //[LabelText("�Զ�")]
        //Auto,
        [LabelText("�ֶ�ָ��")]
        PlayerSelect,
        [LabelText("��ײ���")]
        CollisionSelect,
        //[LabelText("�̶����򳡼��")]
        //AreaSelect,
        [LabelText("����ָ��")]
        ConditionSelect,
        [LabelText("�Զ���")]
        Custom,
    }
}