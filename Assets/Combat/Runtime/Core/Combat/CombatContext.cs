using System.Collections.Generic;

namespace LccModel
{
    public class CombatContext : Entity
    {
        public static CombatContext Instance { get; set; }

        public Dictionary<long, Combat> combatEntityDict = new Dictionary<long, Combat>();
        public Dictionary<long, AbilityItem> abilityItemDict = new Dictionary<long, AbilityItem>();
        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public Combat AddCombatEntity()
        {
            var combatEntity = AddChildren<Combat>();
            if (!combatEntityDict.ContainsKey(combatEntity.InstanceId))
            {
                combatEntityDict.Add(combatEntity.InstanceId, combatEntity);
            }
            return combatEntity;
        }
        public void RemoveCombatEntity(Combat combatEntity)
        {
            if (combatEntityDict.ContainsKey(combatEntity.InstanceId))
            {
                combatEntityDict.Remove(combatEntity.InstanceId);
            }
        }
        public Combat GetCombatEntity(long instanceId)
        {
            if (combatEntityDict.TryGetValue(instanceId, out var combatEntity))
            {
                return combatEntity;
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