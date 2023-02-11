namespace LccModel
{
    public interface IActionAbility
    {
        public CombatEntity OwnerEntity { get; }
        public bool Enable { get; set; }
    }
}