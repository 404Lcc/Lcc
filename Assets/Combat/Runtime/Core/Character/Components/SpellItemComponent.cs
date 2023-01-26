namespace LccModel
{
    public class SpellItemComponent : Component
    {
        private CombatEntity CombatEntity => GetParent<CombatEntity>();
        public override bool DefaultEnable { get; set; } = true;


        public void SpellItemWithTarget(ItemAbility itemAbility, CombatEntity targetEntity)
        {
            if (CombatEntity.SpellItemActionAbility.TryMakeAction(out var spellAction))
            {
                spellAction.ItemAbility = itemAbility;
                spellAction.Target = targetEntity;
                spellAction.UseItem();
            }
            
        }
    }
}