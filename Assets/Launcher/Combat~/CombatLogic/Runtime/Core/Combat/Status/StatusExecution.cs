namespace LccModel
{
    public abstract class StatusExecution : Entity, IAbilityExecution
    {
        public Entity Ability { get; set; }
        public Combat Owner { get; }


        public void BeginExecute()
        {
        }

        public void EndExecute()
        {
        }
    }
}