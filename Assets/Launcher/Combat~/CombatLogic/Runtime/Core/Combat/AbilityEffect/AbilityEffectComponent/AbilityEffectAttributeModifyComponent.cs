namespace LccModel
{
    public class AbilityEffectAttributeModifyComponent : Component
    {
        public AttributeModifyEffect AttributeModifyEffect => (AttributeModifyEffect)GetParent<AbilityEffect>().effect;
        public string NumericValueFormula => AttributeModifyEffect.NumericValueFormula;
        public AttributeType AttributeType => AttributeModifyEffect.AttributeType;

        public Combat Owner => GetParent<AbilityEffect>().Owner;

        public float value;



        public override void Awake()
        {
            value = ExpressionUtil.Evaluate<float>(NumericValueFormula, GetParent<AbilityEffect>().GetParamsDict());

            if (AttributeModifyEffect.ModifyType == ModifyType.Add)
            {
                Owner.GetComponent<AttributeComponent>().GetNumeric(AttributeType).FinalAdd += value;
            }
            if (AttributeModifyEffect.ModifyType == ModifyType.PercentAdd)
            {
                Owner.GetComponent<AttributeComponent>().GetNumeric(AttributeType).FinalPctAdd += value;
            }

        }

        public override void OnDestroy()
        {
            if (AttributeModifyEffect.ModifyType == ModifyType.Add)
            {
                Owner.GetComponent<AttributeComponent>().GetNumeric(AttributeType).FinalAdd -= value;
            }
            if (AttributeModifyEffect.ModifyType == ModifyType.PercentAdd)
            {
                Owner.GetComponent<AttributeComponent>().GetNumeric(AttributeType).FinalPctAdd -= value;
            }
        }
    }
}