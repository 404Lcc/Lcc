using System.Collections.Generic;

namespace LccModel
{
    public class CombatContext : Entity
    {
        public static CombatContext Instance { get; set; }

        public Dictionary<long, Combat> combatDict = new Dictionary<long, Combat>();
        public Dictionary<long, AbilityItem> abilityItemDict = new Dictionary<long, AbilityItem>();
        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public Combat AddCombat()
        {
            var combat = AddChildren<Combat>();
            if (!combatDict.ContainsKey(combat.InstanceId))
            {
                combatDict.Add(combat.InstanceId, combat);
            }
            return combat;
        }
        public void RemoveCombat(Combat combat)
        {
            if (combatDict.ContainsKey(combat.InstanceId))
            {
                combatDict.Remove(combat.InstanceId);
            }
        }
        public Combat GetCombat(long instanceId)
        {
            if (combatDict.TryGetValue(instanceId, out var combat))
            {
                return combat;
            }
            return null;
        }

        public AbilityItem AddAbilityItem(SkillExecution skillExecution, ExecuteClipData data)
        {
            AbilityItem abilityItem = AddChildren<AbilityItem, SkillExecution, ExecuteClipData>(skillExecution, data);


            if (!abilityItemDict.ContainsKey(abilityItem.InstanceId))
            {
                abilityItemDict.Add(abilityItem.InstanceId, abilityItem);
            }
            return abilityItem;
        }
        public void RemoveAbilityItem(AbilityItem abilityItem)
        {
            if (abilityItemDict.ContainsKey(abilityItem.InstanceId))
            {
                abilityItemDict.Remove(abilityItem.InstanceId);
            }
        }
        public AbilityItem GetAbilityItem(long instanceId)
        {
            if (abilityItemDict.TryGetValue(instanceId, out var abilityItem))
            {
                return abilityItem;
            }
            return null;
        }
    }
}