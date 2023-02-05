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

            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        public int GetDamageValue()
        {
            return ParseDamage();
        }

        private int ParseDamage()
        {
            return Mathf.CeilToInt(ExpressionUtil.Evaluate<float>(damageValueFormula, GetParent<AbilityEffect>().GetParamsDict()));
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