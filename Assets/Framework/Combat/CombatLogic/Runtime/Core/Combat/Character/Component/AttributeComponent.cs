using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace LccModel
{
    [LabelText("��������")]
    public enum AttributeType
    {
        [LabelText("���գ�")]
        None = 0,

        [LabelText("����ֵ")]
        HealthPoint = 1000,
        [LabelText("����ֵ����")]
        HealthPointMax = 1001,
        [LabelText("������")]
        Attack = 1002,
        [LabelText("����ֵ")]
        Defense = 1003,
        [LabelText("����ǿ��")]
        AbilityPower = 1004,
        [LabelText("ħ������")]
        SpellResistance = 1005,
        [LabelText("��Ѫ")]
        SuckBlood = 1006,
        [LabelText("��������")]
        CriticalProbability = 1007,
        [LabelText("�ƶ��ٶ�")]
        MoveSpeed = 1008,
        [LabelText("�����ٶ�")]
        AttackSpeed = 1009,
        [LabelText("����ֵ")]
        ShieldValue = 1010,
        [LabelText("����˺�")]
        CauseDamage = 1011,
    }
    public class AttributeComponent : Component
    {
        private readonly Dictionary<AttributeType, FloatNumeric> _attributeDict = new Dictionary<AttributeType, FloatNumeric>();
        public FloatNumeric MoveSpeed => _attributeDict[AttributeType.MoveSpeed];//�ƶ��ٶ�
        public FloatNumeric HealthPoint => _attributeDict[AttributeType.HealthPoint];//��ǰ����ֵ
        public FloatNumeric HealthPointMax => _attributeDict[AttributeType.HealthPointMax];//����ֵ����
        public FloatNumeric Attack => _attributeDict[AttributeType.Attack];//������
        public FloatNumeric Defense => _attributeDict[AttributeType.Defense];//������
        public FloatNumeric AbilityPower => _attributeDict[AttributeType.AbilityPower];//����ǿ��
        public FloatNumeric SpellResistance => _attributeDict[AttributeType.SpellResistance];//ħ������
        public FloatNumeric CriticalProbability => _attributeDict[AttributeType.CriticalProbability];//��������
        public FloatNumeric CauseDamage => _attributeDict[AttributeType.CauseDamage];//��������



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

            var numeric = Parent.AddChildren<FloatNumeric, NumericEntity, AttributeType>(numericEntity, attributeType);

            _attributeDict.Add(attributeType, numeric);

            EventSystem.Instance.Publish(new SyncAttribute(Parent.InstanceId, attributeType));

            numeric.BaseValue = baseValue;

            return numeric;
        }


        public FloatNumeric GetNumeric(AttributeType attributeType)
        {
            return _attributeDict[attributeType];
        }
    }
}