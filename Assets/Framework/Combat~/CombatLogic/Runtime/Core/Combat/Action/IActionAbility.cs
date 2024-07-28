namespace LccModel
{
    public interface IActionAbility
    {
        public bool Enable { get; set; }
        public Combat Owner { get; }
    }
}