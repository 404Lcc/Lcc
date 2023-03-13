namespace LccModel
{
    public class SpellItemComponent : Component
    {
        public Combat CombatEntity => GetParent<Combat>();

        public void SpellItemWithTarget(ItemAbility itemAbility, Combat targetEntity)
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