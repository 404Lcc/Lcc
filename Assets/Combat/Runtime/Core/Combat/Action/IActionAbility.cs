namespace LccModel
{
    public interface IActionAbility
    {
        public bool Enable { get; set; }
        public CombatEntity OwnerEntity { get; }
    }
}