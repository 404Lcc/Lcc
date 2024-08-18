using System.Collections.Generic;

namespace LccModel
{
    public class AttributeViewComponent : Component
    {
        private readonly Dictionary<AttributeType, FloatNumericView> _attributeDict = new Dictionary<AttributeType, FloatNumericView>();
        public FloatNumericView MoveSpeed => _attributeDict[AttributeType.MoveSpeed];//移动速度
        public FloatNumericView HealthPoint => _attributeDict[AttributeType.HealthPoint];//当前生命值
        public FloatNumericView HealthPointMax => _attributeDict[AttributeType.HealthPointMax];//生命值上限
        public FloatNumericView Attack => _attributeDict[AttributeType.Attack];//攻击力
        public FloatNumericView Defense => _attributeDict[AttributeType.Defense];//防御力
        public FloatNumericView AbilityPower => _attributeDict[AttributeType.AbilityPower];//法术强度
        public FloatNumericView SpellResistance => _attributeDict[AttributeType.SpellResistance];//魔法抗性
        public FloatNumericView CriticalProbability => _attributeDict[AttributeType.CriticalProbability];//暴击概率
        public FloatNumericView CauseDamage => _attributeDict[AttributeType.CauseDamage];//暴击概率


        public FloatNumericView AddNumeric(AttributeType attributeType)
        {
            NumericEntity numericEntity = Parent.AddChildren<NumericEntity>();

            var numeric = Parent.AddChildren<FloatNumericView, NumericEntity, AttributeType>(numericEntity, attributeType);

            _attributeDict.Add(attributeType, numeric);

            return numeric;
        }


        public FloatNumericView GetNumeric(AttributeType attributeType)
        {
            return _attributeDict[attributeType];
        }
    }
}