namespace LccModel
{
    public class SpellItemComponent : Component
    {
        public CombatEntity CombatEntity => GetParent<CombatEntity>();

        public void SpellItemWithTarget(ItemAbility itemAbility, CombatEntity targetEntity)
        {
            if (CombatEntity.spellItemActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.itemAbility = itemAbility;
                spellAction.Target = targetEntity;
                spellAction.UseItem();
            }

        }
    }
}