using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class CombatContext : AObjectBase
    {
        public static CombatContext Instance { get; set; }

        public Dictionary<long, Combat> combatDict = new Dictionary<long, Combat>();
        public Dictionary<long, AbilityItem> abilityItemDict = new Dictionary<long, AbilityItem>();
        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public Combat AddCombat(long id, TagType type = TagType.Player)
        {
            Combat combat = AddChildrenWithId<Combat>(id);
            combat.AddComponent<TagComponent, TagType>(type);

            if (!combatDict.ContainsKey(combat.InstanceId))
            {
                combatDict.Add(combat.InstanceId, combat);
            }
            return combat;
        }
        public void RemoveCombat(long instanceId)
        {
            Combat combat = GetCombatByInstanceId(instanceId);
            if (combat != null)
            {
                combat.Dispose();
                combatDict.Remove(instanceId);
            }
        }
        public Combat GetCombatByInstanceId(long instanceId)
        {
            if (combatDict.TryGetValue(instanceId, out var combat))
            {
                return combat;
            }
            return null;
        }
        public Combat GetCombatById(long id)
        {
            var aObjectBase = GetChildren<AObjectBase>(id);
            if (aObjectBase is Combat combat)
            {
                return combat;
            }
            return null;
        }
        public List<Combat> GetCombatListByTag(TagType type)
        {
            List<Combat> list = new List<Combat>();
            foreach (var item in combatDict.Values)
            {
                if (item.TagComponent.tagType == type)
                {
                    list.Add(item);
                }
            }
            return list;
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
        public void RemoveAbilityItem(long instanceId)
        {
            AbilityItem abilityItem = GetAbilityItem(instanceId);
            if (abilityItem != null)
            {
                abilityItem.Dispose();
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