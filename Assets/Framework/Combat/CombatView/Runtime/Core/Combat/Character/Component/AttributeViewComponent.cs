using System.Collections.Generic;

namespace LccModel
{
    public class AttributeViewComponent : Component
    {
        private readonly Dictionary<AttributeType, FloatNumericView> _attributeDict = new Dictionary<AttributeType, FloatNumericView>();
        public FloatNumericView MoveSpeed => _attributeDict[AttributeType.MoveSpeed];//�ƶ��ٶ�
        public FloatNumericView HealthPoint => _attributeDict[AttributeType.HealthPoint];//��ǰ����ֵ
        public FloatNumericView HealthPointMax => _attributeDict[AttributeType.HealthPointMax];//����ֵ����
        public FloatNumericView Attack => _attributeDict[AttributeType.Attack];//������
        public FloatNumericView Defense => _attributeDict[AttributeType.Defense];//������
        public FloatNumericView AbilityPower => _attributeDict[AttributeType.AbilityPower];//����ǿ��
        public FloatNumericView SpellResistance => _attributeDict[AttributeType.SpellResistance];//ħ������
        public FloatNumericView CriticalProbability => _attributeDict[AttributeType.CriticalProbability];//��������
        public FloatNumericView CauseDamage => _attributeDict[AttributeType.CauseDamage];//��������


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