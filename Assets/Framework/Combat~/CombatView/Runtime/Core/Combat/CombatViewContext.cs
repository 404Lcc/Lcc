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
            CombatView combatView = AddChildrenWithId<CombatView, GameObject>(instanceId, new GameObject(instanceId.ToString()));
            return combatView;
        }
        public void RemoveCombatView(long instanceId)
        {
            CombatView combatView = GetCombatView(instanceId);
            if (combatView != null)
            {
                combatView.Dispose();
            }
        }
        public CombatView GetCombatView(long instanceId)
        {
            if (Children.ContainsKey(instanceId))
            {
                var aObjectBase = GetChildren<AObjectBase>(instanceId);
                if (aObjectBase is CombatView combatView)
                {
                    return combatView;
                }
            }
            return null;
        }

        public AbilityItemView AddAbilityItemView(long instanceId)
        {
            AbilityItemView abilityItemView = AddChildrenWithId<AbilityItemView, GameObject>(instanceId, new GameObject(instanceId.ToString()));
            return abilityItemView;
        }
        public void RemoveAbilityItemView(long instanceId)
        {
            AbilityItemView abilityItemView = GetAbilityItemView(instanceId);
            if (abilityItemView != null)
            {
                abilityItemView.Dispose();
            }
        }
        public AbilityItemView GetAbilityItemView(long instanceId)
        {
            if (Children.ContainsKey(instanceId))
            {
                var aObjectBase = GetChildren<AObjectBase>(instanceId);
                if (aObjectBase is AbilityItemView abilityItemView)
                {
                    return abilityItemView;
                }
            }
            return null;
        }
    }
}