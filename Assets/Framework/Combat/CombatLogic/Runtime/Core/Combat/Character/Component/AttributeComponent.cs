using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace LccModel
{
    [LabelText("属性类型")]
    public enum AttributeType
    {
        [LabelText("（空）")]
        None = 0,

        [LabelText("生命值")]
        HealthPoint = 1000,
        [LabelText("生命值上限")]
        HealthPointMax = 1001,
        [LabelText("攻击力")]
        Attack = 1002,
        [LabelText("护甲值")]
        Defense = 1003,
        [LabelText("法术强度")]
        AbilityPower = 1004,
        [LabelText("魔法抗性")]
        SpellResistance = 1005,
        [LabelText("吸血")]
        SuckBlood = 1006,
        [LabelText("暴击概率")]
        CriticalProbability = 1007,
        [LabelText("移动速度")]
        MoveSpeed = 1008,
        [LabelText("攻击速度")]
        AttackSpeed = 1009,
        [LabelText("护盾值")]
        ShieldValue = 1010,
        [LabelText("造成伤害")]
        CauseDamage = 1011,
    }
    public class AttributeComponent : Component
    {
        private readonly Dictionary<string, FloatNumeric> _attributeDict = new Dictionary<string, FloatNumeric>();
        public FloatNumeric MoveSpeed => _attributeDict[nameof(AttributeType.MoveSpeed)];//移动速度
        public FloatNumeric HealthPoint => _attributeDict[nameof(AttributeType.HealthPoint)];//当前生命值
        public FloatNumeric HealthPointMax => _attributeDict[nameof(AttributeType.HealthPointMax)];//生命值上限
        public FloatNumeric Attack => _attributeDict[nameof(AttributeType.Attack)];//攻击力
        public FloatNumeric Defense => _attributeDict[nameof(AttributeType.Defense)];//防御力
        public FloatNumeric AbilityPower => _attributeDict[nameof(AttributeType.AbilityPower)];//法术强度
        public FloatNumeric SpellResistance => _attributeDict[nameof(AttributeType.SpellResistance)];//魔法抗性
        public FloatNumeric CriticalProbability => _attributeDict[nameof(AttributeType.CriticalProbability)];//暴击概率
        public FloatNumeric CauseDamage => _attributeDict[nameof(AttributeType.CauseDamage)];//暴击概率



        public override void Awake()
        {
            base.Awake();

            AddNumeric(AttributeType.HealthPointMax, 1000);
            AddNumeric(AttributeType.HealthPoint, 1000);
            AddNumeric(AttributeType.MoveSpeed, 1);
            AddNumeric(AttributeType.Attack, 40);
            AddNumeric(AttributeType.Defense, 30);
            AddNumeric(AttributeType.CriticalProbability, 0.5f);
            AddNumeric(AttributeType.CauseDamage, 1);
        }
        public FloatNumeric AddNumeric(AttributeType attributeType, float baseValue)
        {
            NumericEntity numericEntity = Parent.AddChildren<NumericEntity>();
            var numeric = Parent.AddChildren<FloatNumeric, NumericEntity, int>(numericEntity, (int)attributeType);
            numeric.SetBase(baseValue);
            _attributeDict.Add(attributeType.ToString(), numeric);
            return numeric;
        }

        public FloatNumeric GetNumeric(string attributeName)
        {
            return _attributeDict[attributeName];
        }
    }
}