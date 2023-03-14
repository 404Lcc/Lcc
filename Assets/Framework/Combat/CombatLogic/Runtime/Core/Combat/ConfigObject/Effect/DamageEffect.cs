using Sirenix.OdinInspector;

namespace LccModel
{
    [LabelText("伤害类型")]
    public enum DamageType
    {
        [LabelText("物理伤害")]
        Physic = 0,
        [LabelText("魔法伤害")]
        Magic = 1,
        [LabelText("真实伤害")]
        Real = 2,
    }
    [Effect("造成伤害", 10)]
    public class DamageEffect : Effect
    {
        public override string Label => "造成伤害";

        [ToggleGroup("Enabled")]
        public DamageType DamageType;

        [ToggleGroup("Enabled"), LabelText("取值")]
        public string DamageValueFormula;

        [ToggleGroup("Enabled"), LabelText("能否暴击")]
        public bool CanCrit;
    }
}