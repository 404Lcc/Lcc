using System.Collections.Generic;
using UnityEngine;

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
            Combat combat = AddChildren<Combat>();
            combat.AddComponent<AABB2DComponent, Vector2, Vector2>(new Vector2(-1, -1), new Vector2(1, 1));

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
                var instanceId = combat.InstanceId;
                combatDict[combat.InstanceId].Dispose();
                combatDict.Remove(instanceId);
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
            abilityItem.AddComponent<AABB2DComponent, Vector2, Vector2>(new Vector2(-1, -1), new Vector2(1, 1));

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
                var instanceId = abilityItem.InstanceId;
                abilityItemDict[abilityItem.InstanceId].Dispose();
                abilityItemDict.Remove(instanceId);
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