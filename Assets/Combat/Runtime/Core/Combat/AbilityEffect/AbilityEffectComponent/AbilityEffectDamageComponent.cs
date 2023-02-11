using UnityEngine;

namespace LccModel
{
    public class AbilityEffectDamageComponent : Component
    {
        public DamageEffect DamageEffect => GetParent<AbilityEffect>().effect as DamageEffect;
        public string DamageValueFormula => DamageEffect.DamageValueFormula;

        public CombatEntity OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;

        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        public int GetDamageValue()
        {
            return Mathf.CeilToInt(ExpressionUtil.Evaluate<float>(DamageValueFormula, GetParent<AbilityEffect>().GetParamsDict()));
        }

        private void OnAssignEffect(Entity entity)
        {
            EffectAssignAction effectAssignAction = (EffectAssignAction)entity;
            if (OwnerEntity.damageActionAbility.TryMakeAction(out var damageAction))
            {
                effectAssignAction.FillDatasToAction(damageAction);
                damageAction.damageSource = DamageSource.Skill;
                damageAction.ApplyDamage();
            }
        }
    }
}