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
        private readonly Dictionary<string, FloatNumeric> attributeDict = new Dictionary<string, FloatNumeric>();
        public FloatNumeric MoveSpeed => attributeDict[nameof(AttributeType.MoveSpeed)];//�ƶ��ٶ�
        public FloatNumeric HealthPoint => attributeDict[nameof(AttributeType.HealthPoint)]; //��ǰ����ֵ
        public FloatNumeric HealthPointMax => attributeDict[nameof(AttributeType.HealthPointMax)]; //����ֵ����
        public FloatNumeric Attack => attributeDict[nameof(AttributeType.Attack)];//������
        public FloatNumeric Defense => attributeDict[nameof(AttributeType.Defense)];//�����������ף�
        public FloatNumeric AbilityPower => attributeDict[nameof(AttributeType.AbilityPower)];//����ǿ��
        public FloatNumeric SpellResistance => attributeDict[nameof(AttributeType.SpellResistance)];//ħ������
        public FloatNumeric CriticalProbability => attributeDict[nameof(AttributeType.CriticalProbability)];//��������
        public FloatNumeric CauseDamage => attributeDict[nameof(AttributeType.CauseDamage)]; //��������



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
            attributeDict.Add(attributeType.ToString(), numeric);
            return numeric;
        }

        public FloatNumeric GetNumeric(string attributeName)
        {
            return attributeDict[attributeName];
        }
    }
}