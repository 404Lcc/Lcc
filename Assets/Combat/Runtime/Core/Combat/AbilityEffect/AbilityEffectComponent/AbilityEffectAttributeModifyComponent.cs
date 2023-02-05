namespace LccModel
{
    public class AbilityEffectAttributeModifyComponent : Component
    {
        public AttributeModifyEffect attributeModifyEffect;
        public string numericValueFormula;
        public AttributeType attributeType;

        public float value;



        public override void Awake()
        {
            attributeModifyEffect = GetParent<AbilityEffect>().effectConfig as AttributeModifyEffect;
            numericValueFormula = attributeModifyEffect.NumericValueFormula;
            attributeType = attributeModifyEffect.AttributeType;

            CombatEntity parentEntity = Parent.GetParent<StatusAbility>().GetParent<CombatEntity>();
            value = ExpressionUtil.Evaluate<float>(numericValueFormula, GetParent<AbilityEffect>().GetParamsDict());


            if (attributeModifyEffect.ModifyType == ModifyType.Add)
            {
                parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType.ToString()).AddFinalAddModifier(value);
            }
            if (attributeModifyEffect.ModifyType == ModifyType.PercentAdd)
            {
                parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType.ToString()).AddFinalPctAddModifier(value);
            }

        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            var parentEntity = Parent.GetParent<StatusAbility>().GetParent<CombatEntity>();
            var attributeType = attributeModifyEffect.AttributeType.ToString();
            if (attributeModifyEffect.ModifyType == ModifyType.Add)
            {
                parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType).RemoveFinalAddModifier(value);
            }
            if (attributeModifyEffect.ModifyType == ModifyType.PercentAdd)
            {
                parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType).RemoveFinalPctAddModifier(value);
            }
        }
    }
}