namespace LccModel
{
    public class SpellItemComponent : Component
    {
        public Combat Combat => GetParent<Combat>();

        public void SpellItemWithTarget(ItemAbility itemAbility, Combat target)
        {
            if (Combat.spellItemActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.itemAbility = itemAbility;
                spellAction.Target = target;
                spellAction.UseItem();
            }

        }
    }
}