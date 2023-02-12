using System.Collections.Generic;

namespace LccModel
{
    public class AbilityEffectConditionTriggerComponent : Component
    {
        public Effect Effect => GetParent<AbilityEffect>().effect;
        public string ConditionValueFormula => ParseParams(Effect.ConditionValueFormula, GetParent<AbilityEffect>().GetParamsDict());
        public ConditionType ConditionType => Effect.ConditionType;

        public CombatEntity OwnerEntity => GetParent<AbilityEffect>().OwnerEntity;

        public override void Awake()
        {
            OwnerEntity.ListenerCondition(ConditionType, OnConditionTrigger, ConditionValueFormula);
        }
        public override void OnDestroy()
        {
            OwnerEntity.UnListenCondition(ConditionType, OnConditionTrigger);
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