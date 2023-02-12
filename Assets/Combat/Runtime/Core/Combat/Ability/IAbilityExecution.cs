namespace LccModel
{
    public interface IAbilityExecution
    {
        public Entity AbilityEntity { get; set; }
        public CombatEntity OwnerEntity { get; }

        public void BeginExecute();
        public void EndExecute();
    }
}