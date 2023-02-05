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
        public ConditionType conditionType;


        public override void Awake()
        {
            base.Awake();

            effect = GetParent<AbilityEffect>().effectConfig;
            conditionValueFormula = ParseParams(effect.ConditionParams, GetParent<AbilityEffect>().GetParamsDict());
            conditionType = GetParent<AbilityEffect>().effectConfig.ConditionType;
            Parent.GetParent<StatusAbility>().OwnerEntity.ListenerCondition(conditionType, OnConditionTrigger, conditionValueFormula);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            Parent.GetParent<StatusAbility>().OwnerEntity.UnListenCondition(conditionType, OnConditionTrigger);
        }

        private void OnConditionTrigger()
        {
            GetParent<AbilityEffect>().TryAssignEffectToOwner();
        }

        private string ParseParams(string origin, Dictionary<string, string> paramsDict)
        {
            string temp = origin;
            foreach (var item in paramsDict)
            {
                if (!string.IsNullOrEmpty(temp))
                {
                    temp = temp.Replace(item.Key, item.Value);
                }
            }
            return temp;
        }
    }
}