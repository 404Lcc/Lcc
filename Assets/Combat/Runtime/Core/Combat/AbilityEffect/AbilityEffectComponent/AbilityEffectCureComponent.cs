using UnityEngine;

namespace LccModel
{
    public class AbilityEffectCureComponent : Component
    {
        public CureEffect CureEffect => (CureEffect)GetParent<AbilityEffect>().effect;
        public string CureValueFormula => CureEffect.CureValueFormula;
        public Combat OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;




        public int GetCureValue()
        {
            return Mathf.CeilToInt(ExpressionUtil.Evaluate<float>(CureValueFormula, GetParent<AbilityEffect>().GetParamsDict()));
        }

        public void OnAssignEffect(EffectAssignAction effectAssignAction)
        {
            if (OwnerEntity.cureActionAbility.TryMakeAction(out var action))
            {
                effectAssignAction.FillDatasToAction(action);
                action.ApplyCure();
            }
        }
    }
}