using Sirenix.OdinInspector;

namespace LccModel
{
    public enum EffectTriggerType
    {
        [LabelText("（空）")]
        None = 0,
        [LabelText("立即触发")]
        Instant = 1,
        [LabelText("条件触发")]
        Condition = 2,
        [LabelText("行动点触发")]
        Action = 3,
        [LabelText("间隔触发")]
        Interval = 4,
        [LabelText("在行动点且满足条件")]
        ActionCondition = 5,
    }
}