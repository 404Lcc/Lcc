using Sirenix.OdinInspector;
using System;

namespace LccModel
{
    [Flags]
    [LabelText("行为禁制")]
    public enum ActionControlType
    {
        [LabelText("（空）")]
        None = 0,
        [LabelText("移动禁止")]
        MoveForbid = 1 << 1,
        [LabelText("施法禁止")]
        SkillForbid = 1 << 2,
        [LabelText("攻击禁止")]
        AttackForbid = 1 << 3,
        [LabelText("移动控制")]
        MoveControl = 1 << 4,
        [LabelText("攻击控制")]
        AttackControl = 1 << 5,
    }
}