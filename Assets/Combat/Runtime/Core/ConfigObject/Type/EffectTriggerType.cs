using Sirenix.OdinInspector;

namespace LccModel
{
    public enum EffectTriggerType
    {
        [LabelText("���գ�")]
        None = 0,
        [LabelText("��������")]
        Instant = 1,
        [LabelText("��������")]
        Condition = 2,
        [LabelText("�ж��㴥��")]
        Action = 3,
        [LabelText("�������")]
        Interval = 4,
        [LabelText("���ж�������������")]
        ActionCondition = 5,
    }
}