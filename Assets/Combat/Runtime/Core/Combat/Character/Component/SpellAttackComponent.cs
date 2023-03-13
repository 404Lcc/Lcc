namespace LccModel
{
    public class SpellAttackComponent : Component
    {
        public Combat CombatEntity => GetParent<Combat>();

        public void SpellAttackWithTarget(Combat targetEntity)
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