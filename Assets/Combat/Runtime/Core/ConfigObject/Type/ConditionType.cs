using Sirenix.OdinInspector;

namespace LccModel
{
    public enum ConditionType
    {
        [LabelText("�Զ�������")]
        CustomCondition = 0,
        [LabelText("������ֵ����x")]
        WhenHPLower = 1,
        [LabelText("������ֵ���ڰٷֱ�x")]
        WhenHPPctLower = 2,
        [LabelText("��x����û������")]
        WhenInTimeNoDamage = 3,
    }
}