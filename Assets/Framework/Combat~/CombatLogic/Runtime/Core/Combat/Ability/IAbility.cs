namespace LccModel
{
    public interface IAbility
    {
        public bool Enable { get; set; }
        public Combat Owner { get; }

        public void ActivateAbility();
        public void EndAbility();
        public Entity CreateExecution();
    }
}