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


            GetParent<AbilityEffect>().ParseParams();

            ((Entity)Parent).OnEvent(nameof(AbilityEffect.StartAssignEffect), OnAssignEffect);
        }

        public int GetCureValue()
        {
            return ParseValue();
        }

        private int ParseValue()
        {
            var expression = ExpressionHelper.TryEvaluate(cureValueFormula);
            if (expression.Parameters.ContainsKey("生命值上限"))
            {
                expression.Parameters["生命值上限"].Value = GetParent<AbilityEffect>().OwnerEntity.GetComponent<AttributeComponent>().HealthPoint.Value;
            }
            return Mathf.CeilToInt((float)expression.Value);
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