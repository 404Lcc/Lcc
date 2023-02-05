namespace LccModel
{
    public interface IAbilityExecution
    {
        public Entity AbilityEntity
        {
            get;
            set;
        }
        public CombatEntity OwnerEntity
        {
            get;
            set;
        }

        public void BeginExecute();
        public void EndExecute();
    }
}