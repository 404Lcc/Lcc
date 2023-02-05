namespace LccModel
{
    public class AbilityEffectAttributeModifyComponent : Component
    {
        public AttributeModifyEffect attributeModifyEffect;
        public string numericValueFormula;
        public float value;


        public override void Awake()
        {
            attributeModifyEffect = GetParent<AbilityEffect>().effectConfig as AttributeModifyEffect;

            var parentEntity = Parent.GetParent<StatusAbility>().GetParent<CombatEntity>();

            var numericValue = this.numericValueFormula;
            numericValue = numericValue.Replace("%", "");
            var expression = ExpressionHelper.TryEvaluate(numericValue);
            var value = (float)expression.Value;

            var attributeType = attributeModifyEffect.AttributeType.ToString();
            if (attributeModifyEffect.ModifyType == ModifyType.Add)
            {
                parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType).AddFinalAddModifier(value);
            }
            if (attributeModifyEffect.ModifyType == ModifyType.PercentAdd)
            {
                parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType).AddFinalPctAddModifier(value);
            }
            this.value = value;
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