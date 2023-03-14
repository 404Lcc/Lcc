namespace LccModel
{
    public class SpellAttackComponent : Component
    {
        public Combat Combat => GetParent<Combat>();

        public void SpellAttackWithTarget(Combat target)
        {
            if (Combat.spellingAttackExecution != null) return;

            if (Combat.spellAttackActionAbility.TryMakeAction(out var action))
            {
                action.Target = target;
                action.ApplyAttack();
            }
        }
    }
}