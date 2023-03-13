using UnityEngine;

namespace LccModel
{
    public class AbilityEffectDamageComponent : Component
    {
        public DamageEffect DamageEffect => GetParent<AbilityEffect>().effect as DamageEffect;
        public string DamageValueFormula => DamageEffect.DamageValueFormula;

        public Combat OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;



        public int GetDamageValue()
        {
            return Mathf.CeilToInt(ExpressionUtil.Evaluate<float>(DamageValueFormula, GetParent<AbilityEffect>().GetParamsDict()));
        }

        public void OnAssignEffect(EffectAssignAction effectAssignAction)
        {
            if (OwnerEntity.damageActionAbility.TryMakeAction(out var damageAction))
            {
                effectAssignAction.FillDatasToAction(damageAction);
                damageAction.damageSource = DamageSource.Skill;
                damageAction.ApplyDamage();
            }
        }
    }
}