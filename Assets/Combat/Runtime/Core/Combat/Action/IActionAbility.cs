namespace LccModel
{
    public interface IActionAbility
    {
        public CombatEntity OwnerEntity { get; set; }
        public bool Enable { get; set; }
    }
}