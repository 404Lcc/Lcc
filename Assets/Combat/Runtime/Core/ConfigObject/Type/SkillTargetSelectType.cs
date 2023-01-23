using Sirenix.OdinInspector;

namespace LccModel
{
    [LabelText("效果应用目标选择方式")]
    public enum SkillTargetSelectType
    {
        //[LabelText("自动")]
        //Auto,
        [LabelText("手动指定")]
        PlayerSelect,
        [LabelText("碰撞检测")]
        CollisionSelect,
        //[LabelText("固定区域场检测")]
        //AreaSelect,
        [LabelText("条件指定")]
        ConditionSelect,
        [LabelText("自定义")]
        Custom,
    }
}