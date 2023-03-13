using System.Collections.Generic;

namespace LccModel
{
    public class CombatViewContext : Entity
    {
        public static CombatViewContext Instance { get; set; }

        public Dictionary<long, CombatView> combatViewDict = new Dictionary<long, CombatView>();
        public Dictionary<long, AbilityItemView> abilityItemViewDict = new Dictionary<long, AbilityItemView>();
        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public CombatView AddCombatView(long instanceId)
        {
            var combatView = AddChildrenWithId<CombatView>(instanceId);
            if (!combatViewDict.ContainsKey(combatView.Id))
            {
                combatViewDict.Add(combatView.Id, combatView);
            }
            return combatView;
        }
        public void RemoveCombatView(long instanceId)
        {
            if (combatViewDict.ContainsKey(instanceId))
            {
                combatViewDict.Remove(instanceId);
            }
        }
        public CombatView GetCombatView(long instanceId)
        {
            if (combatViewDict.TryGetValue(instanceId, out var combatView))
            {
                return combatView;
            }
            return null;
        }

        public AbilityItemView AddAbilityItemView(long instanceId)
        {
            AbilityItemView abilityItemView = AddChildrenWithId<AbilityItemView>(instanceId);

            if (!abilityItemViewDict.ContainsKey(abilityItemView.Id))
            {
                abilityItemViewDict.Add(abilityItemView.Id, abilityItemView);
            }
            return abilityItemView;
        }
        public void RemoveAbilityItemView(long instanceId)
        {
            if (abilityItemViewDict.ContainsKey(instanceId))
            {
                abilityItemViewDict.Remove(instanceId);
            }
        }
        public AbilityItemView GetAbilityItemView(long instanceId)
        {
            if (abilityItemViewDict.TryGetValue(instanceId, out var abilityItemView))
            {
                return abilityItemView;
            }
            return null;
        }
    }
}