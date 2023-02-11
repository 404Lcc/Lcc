namespace LccModel
{
    public abstract class StatusExecution : Entity, IAbilityExecution
    {
        public Entity AbilityEntity { get; set; }
        public CombatEntity OwnerEntity { get; set; }


        public void BeginExecute()
        {
        }

        public void EndExecute()
        {
        }
    }
}