namespace LccModel
{
    public interface IAbilityEntity
    {
        public bool Enable { get; set; }
        public CombatEntity OwnerEntity { get; }

        public void ActivateAbility();
        public void EndAbility();
        public Entity CreateExecution();
    }
}