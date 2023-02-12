namespace LccModel
{
    public class AbilityEffectAttributeModifyComponent : Component
    {
        public AttributeModifyEffect AttributeModifyEffect => (AttributeModifyEffect)GetParent<AbilityEffect>().effect;
        public string NumericValueFormula => AttributeModifyEffect.NumericValueFormula;
        public AttributeType AttributeType => AttributeModifyEffect.AttributeType;

        public CombatEntity OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;

        public float value;



        public override void Awake()
        {
            value = ExpressionUtil.Evaluate<float>(NumericValueFormula, GetParent<AbilityEffect>().GetParamsDict());

            if (AttributeModifyEffect.ModifyType == ModifyType.Add)
            {
                OwnerEntity.GetComponent<AttributeComponent>().GetNumeric(AttributeType.ToString()).AddFinalAddModifier(value);
            }
            if (AttributeModifyEffect.ModifyType == ModifyType.PercentAdd)
            {
                OwnerEntity.GetComponent<AttributeComponent>().GetNumeric(AttributeType.ToString()).AddFinalPctAddModifier(value);
            }

        }

        public override void OnDestroy()
        {
            if (AttributeModifyEffect.ModifyType == ModifyType.Add)
            {
                OwnerEntity.GetComponent<AttributeComponent>().GetNumeric(AttributeType.ToString()).RemoveFinalAddModifier(value);
            }
            if (AttributeModifyEffect.ModifyType == ModifyType.PercentAdd)
            {
                OwnerEntity.GetComponent<AttributeComponent>().GetNumeric(AttributeType.ToString()).RemoveFinalPctAddModifier(value);
            }
        }
    }
}