using UnityEngine;

namespace LccModel
{
    public class AbilityEffectCureComponent : Component
    {
        public CureEffect CureEffect => (CureEffect)GetParent<AbilityEffect>().effect;
        public string CureValueFormula => CureEffect.CureValueFormula;
        public Combat Owner => GetParent<AbilityEffect>().Owner;




        public int GetCureValue()
        {
            return Mathf.CeilToInt(ExpressionUtil.Evaluate<float>(CureValueFormula, GetParent<AbilityEffect>().GetParamsDict()));
        }

        public void OnAssignEffect(EffectAssignAction effectAssignAction)
        {
            if (Owner.cureActionAbility.TryMakeAction(out var action))
            {
                effectAssignAction.FillDatasToAction(action);
                action.ApplyCure();
            }
        }
    }
}