using System.Collections.Generic;

namespace LccModel
{
    /// <summary>
    /// 条件触发组件
    /// </summary>
    public class AbilityEffectConditionTriggerComponent : Component
    {
        public Effect effect;
        public string conditionValueFormula;


        public override void Awake()
        {
            base.Awake();


            effect = GetParent<AbilityEffect>().effectConfig;
            conditionValueFormula = effect.ConditionParams;

            var paramsDict = GetParent<AbilityEffect>().GetParamsDict();
            conditionValueFormula = ParseParams(conditionValueFormula, paramsDict);

            var conditionType = GetParent<AbilityEffect>().effectConfig.ConditionType;
            Parent.GetParent<StatusAbility>().OwnerEntity.ListenerCondition(conditionType, OnConditionTrigger, conditionValueFormula);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            var conditionType = GetParent<AbilityEffect>().effectConfig.ConditionType;
            Parent.GetParent<StatusAbility>().OwnerEntity.UnListenCondition(conditionType, OnConditionTrigger);
        }

        private void OnConditionTrigger()
        {
            this.GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }

        private string ParseParams(string origin, Dictionary<string, string> paramsDict)
        {
            foreach (var item in paramsDict)
            {
                if (!string.IsNullOrEmpty(origin))
                {
                    origin = origin.Replace(item.Key, item.Value);
                }
            }
            return origin;
        }
    }
}