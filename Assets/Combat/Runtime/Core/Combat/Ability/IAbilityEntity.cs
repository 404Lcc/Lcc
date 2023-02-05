namespace LccModel
{
    public interface IAbilityEntity
    {
        public CombatEntity OwnerEntity
        {
            get;
            set;
        }
        public CombatEntity ParentEntity
        {
            get;
        }
        public bool Enable
        {
            get; set;
        }

        public void ActivateAbility();
        public void EndAbility();
        public Entity CreateExecution();
    }
}