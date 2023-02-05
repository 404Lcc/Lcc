using UnityEngine;

namespace LccModel
{
    public class AbilityEffectCureComponent : Component
    {
        public CureEffect cureEffect;
        public string cureValueFormula;


        public override void Awake()
        {

            cureEffect = GetParent<AbilityEffect>().effectConfig as CureEffect;
            cureValueFormula = cureEffect.CureValueFormula;


            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        public int GetCureValue()
        {
            return ParseValue();
        }

        private int ParseValue()
        {
            return Mathf.CeilToInt(ExpressionHelper.Evaluate<float>(cureValueFormula, GetParent<AbilityEffect>().GetParamsDict()));
        }

        private void OnAssignEffect(Entity entity)
        {
            var effectAssignAction = entity as EffectAssignAction;
            if (GetParent<AbilityEffect>().OwnerEntity.cureActionAbility.TryMakeAction(out var action))
            {
                effectAssignAction.FillDatasToAction(action);
                action.ApplyCure();
            }
        }
    }
}