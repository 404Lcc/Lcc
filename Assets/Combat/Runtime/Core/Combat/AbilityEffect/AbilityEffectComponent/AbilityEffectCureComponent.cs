using UnityEngine;

namespace LccModel
{
    public class AbilityEffectCureComponent : Component
    {
        public CureEffect CureEffect => (CureEffect)GetParent<AbilityEffect>().effect;
        public string CureValueFormula => CureEffect.CureValueFormula;
        public CombatEntity OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;


        public override void Awake()
        {
            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        public int GetCureValue()
        {
            return Mathf.CeilToInt(ExpressionUtil.Evaluate<float>(CureValueFormula, GetParent<AbilityEffect>().GetParamsDict()));
        }

        private void OnAssignEffect(Entity entity)
        {
            EffectAssignAction effectAssignAction = (EffectAssignAction)entity;
            if (OwnerEntity.cureActionAbility.TryMakeAction(out var action))
            {
                effectAssignAction.FillDatasToAction(action);
                action.ApplyCure();
            }
        }
    }
}