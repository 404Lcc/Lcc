using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class CombatViewContext : Entity
    {
        public static CombatViewContext Instance { get; set; }

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public CombatView AddCombatView(long instanceId)
        {
            CombatView combatView = AddChildrenWithId<CombatView>(instanceId);
            combatView.AddComponent<GameObjectComponent, GameObject>(null);
            return combatView;
        }
        public void RemoveCombatView(long instanceId)
        {
            if (Children.ContainsKey(instanceId))
            {
                GetChildren<CombatView>(instanceId).Dispose();
            }
        }
        public CombatView GetCombatView(long instanceId)
        {
            if (Children.ContainsKey(instanceId))
            {
                return GetChildren<CombatView>(instanceId);
            }
            return null;
        }

        public AbilityItemView AddAbilityItemView(long instanceId)
        {
            AbilityItemView abilityItemView = AddChildrenWithId<AbilityItemView>(instanceId);
            abilityItemView.AddComponent<GameObjectComponent, GameObject>(null);
            return abilityItemView;
        }
        public void RemoveAbilityItemView(long instanceId)
        {
            if (Children.ContainsKey(instanceId))
            {
                GetChildren<AbilityItemView>(instanceId).Dispose();
            }
        }
        public AbilityItemView GetAbilityItemView(long instanceId)
        {
            if (Children.ContainsKey(instanceId))
            {
                return GetChildren<AbilityItemView>(instanceId);
            }
            return null;
        }
    }
}