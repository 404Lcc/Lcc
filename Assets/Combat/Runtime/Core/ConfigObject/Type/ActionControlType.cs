using Sirenix.OdinInspector;
using System;

namespace LccModel
{
    [Flags]
    [LabelText("��Ϊ����")]
    public enum ActionControlType
    {
        [LabelText("���գ�")]
        None = 0,
        [LabelText("�ƶ���ֹ")]
        MoveForbid = 1 << 1,
        [LabelText("ʩ����ֹ")]
        SkillForbid = 1 << 2,
        [LabelText("������ֹ")]
        AttackForbid = 1 << 3,
        [LabelText("�ƶ�����")]
        MoveControl = 1 << 4,
        [LabelText("��������")]
        AttackControl = 1 << 5,
    }
}