namespace LccModel
{
    public class SpellAttackComponent : Component
    {
        private CombatEntity CombatEntity => GetParent<CombatEntity>();
        public override bool DefaultEnable => true;


        public void SpellAttackWithTarget(CombatEntity targetEntity)
        {
            if (CombatEntity.SpellingAttackExecution != null) return;

            if (CombatEntity.SpellAttackActionAbility.TryMakeAction(out var action))
            {
                action.Target = targetEntity;
                action.ApplyAttack();
            }
        }
    }
}