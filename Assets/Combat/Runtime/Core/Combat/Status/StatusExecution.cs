namespace LccModel
{
    public abstract class StatusExecution : Entity, IAbilityExecution
    {
        public Entity AbilityEntity { get; set; }
        public Combat OwnerEntity { get; }


        public void BeginExecute()
        {
        }

        public void EndExecute()
        {
        }
    }
}