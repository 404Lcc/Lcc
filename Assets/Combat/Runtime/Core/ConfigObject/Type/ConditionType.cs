using Sirenix.OdinInspector;

namespace LccModel
{
    public enum ConditionType
    {
        [LabelText("自定义条件")]
        CustomCondition = 0,
        [LabelText("当生命值低于x")]
        WhenHPLower = 1,
        [LabelText("当生命值低于百分比x")]
        WhenHPPctLower = 2,
        [LabelText("当x秒内没有受伤")]
        WhenInTimeNoDamage = 3,
    }
}