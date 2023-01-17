using UnityEngine;

namespace LccModel
{
    public class EffectDamageComponent : Component
    {
        public DamageEffect DamageEffect { get; set; }
        public string DamageValueFormula { get; set; }


        public override void Awake()
        {
            DamageEffect = GetParent<AbilityEffect>().EffectConfig as DamageEffect;
            DamageValueFormula = DamageEffect.DamageValueFormula;
            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        public int GetDamageValue()
        {
            return ParseDamage();
        }

        private int ParseDamage()
        {
            var expression = ExpressionHelper.ExpressionParser.EvaluateExpression(DamageValueFormula);
            if (expression.Parameters.ContainsKey("自身攻击力"))
            {
                expression.Parameters["自身攻击力"].Value = GetParent<AbilityEffect>().OwnerEntity.GetComponent<AttributeComponent>().Attack.Value;
            }
            return Mathf.CeilToInt((float)expression.Value);
        }

        private void OnAssignEffect(Entity entity)
        {
            var effectAssignAction = entity as EffectAssignAction;
            if (GetParent<AbilityEffect>().OwnerEntity.DamageAbility.TryMakeAction(out var damageAction))
            {
                effectAssignAction.FillDatasToAction(damageAction);
                damageAction.DamageSource = DamageSource.Skill;
                damageAction.ApplyDamage();
            }
        }
    }
}