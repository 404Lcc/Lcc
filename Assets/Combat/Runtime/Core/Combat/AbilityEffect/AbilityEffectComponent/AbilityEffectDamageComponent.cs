using UnityEngine;

namespace LccModel
{
    public class AbilityEffectDamageComponent : Component
    {
        public DamageEffect damageEffect;
        public string damageValueFormula;


        public override void Awake()
        {
            damageEffect = GetParent<AbilityEffect>().effectConfig as DamageEffect;
            damageValueFormula = damageEffect.DamageValueFormula;


            GetParent<AbilityEffect>().ParseParams();

            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        public int GetDamageValue()
        {
            return ParseDamage();
        }

        private int ParseDamage()
        {
            var expression = ExpressionHelper.TryEvaluate(damageValueFormula);
            if (expression.Parameters.ContainsKey("自身攻击力"))
            {
                expression.Parameters["自身攻击力"].Value = GetParent<AbilityEffect>().OwnerEntity.GetComponent<AttributeComponent>().Attack.Value;
            }
            return Mathf.CeilToInt((float)expression.Value);
        }

        private void OnAssignEffect(Entity entity)
        {
            var effectAssignAction = entity as EffectAssignAction;
            if (GetParent<AbilityEffect>().OwnerEntity.damageActionAbility.TryMakeAction(out var damageAction))
            {
                effectAssignAction.FillDatasToAction(damageAction);
                damageAction.damageSource = DamageSource.Skill;
                damageAction.ApplyDamage();
            }
        }
    }
}