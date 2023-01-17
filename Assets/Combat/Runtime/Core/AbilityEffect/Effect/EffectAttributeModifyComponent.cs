namespace LccModel
{
    public class EffectAttributeModifyComponent : Component
    {
        public override bool DefaultEnable => false;
        public AttributeModifyEffect AttributeModifyEffect { get; set; }
        public float AttributeModifier { get; set; }
        public string ModifyValueFormula { get; set; }


        public override void Awake()
        {
            AttributeModifyEffect = GetParent<AbilityEffect>().EffectConfig as AttributeModifyEffect;
        }

        public override void OnEnable()
        {
            //var parentEntity = Parent.GetParent<StatusAbility>().GetParent<CombatEntity>();
            //var attributeModifyEffect = AttributeModifyEffect;
            //var numericValue = ModifyValueFormula;
            //numericValue = numericValue.Replace("%", "");
            //var expression = ExpressionHelper.ExpressionParser.EvaluateExpression(numericValue);
            //var value = (float)expression.Value;

            //var attributeType = attributeModifyEffect.AttributeType.ToString();
            ////Log.Debug($"EffectAttributeModifyComponent OnEnable {attributeType} {numericValue} {attributeModifyEffect.ModifyType}");
            //if (attributeModifyEffect.ModifyType == ModifyType.Add)
            //{
            //    parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType).AddFinalAddModifier(numericModifier);
            //}
            //if (attributeModifyEffect.ModifyType == ModifyType.PercentAdd)
            //{
            //    parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType).AddFinalPctAddModifier(numericModifier);
            //}
            //AttributeModifier = numericModifier;
        }

        public override void OnDisable()
        {
            //var parentEntity = Parent.GetParent<StatusAbility>().GetParent<CombatEntity>();
            //var attributeType = AttributeModifyEffect.AttributeType.ToString();
            ////Log.Debug($"EffectAttributeModifyComponent OnDisable {attributeType} {AttributeModifyEffect.NumericValue} {AttributeModifyEffect.ModifyType}");
            //if (AttributeModifyEffect.ModifyType == ModifyType.Add)
            //{
            //    parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType).RemoveFinalAddModifier(AttributeModifier);
            //}
            //if (AttributeModifyEffect.ModifyType == ModifyType.PercentAdd)
            //{
            //    parentEntity.GetComponent<AttributeComponent>().GetNumeric(attributeType).RemoveFinalPctAddModifier(AttributeModifier);
            //}
        }
    }
}