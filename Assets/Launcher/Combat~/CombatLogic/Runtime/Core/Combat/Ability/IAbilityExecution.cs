namespace LccModel
{
    public interface IAbilityExecution
    {
        public Entity Ability { get; set; }
        public Combat Owner { get; }

        public void BeginExecute();
        public void EndExecute();
    }
}