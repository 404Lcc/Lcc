namespace LccModel
{
    public class SpellAttackComponent : Component
    {
        public override bool DefaultEnable => true;
        public CombatEntity CombatEntity => GetParent<CombatEntity>();

        public void SpellAttackWithTarget(CombatEntity targetEntity)
        {
            if (CombatEntity.spellingAttackExecution != null) return;

            if (CombatEntity.spellAttackActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.ApplyAttack();
            }
        }
    }
}