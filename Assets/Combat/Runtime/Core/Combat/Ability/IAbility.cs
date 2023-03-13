namespace LccModel
{
    public interface IAbility
    {
        public bool Enable { get; set; }
        public Combat OwnerEntity { get; }

        public void ActivateAbility();
        public void EndAbility();
        public Entity CreateExecution();
    }
}