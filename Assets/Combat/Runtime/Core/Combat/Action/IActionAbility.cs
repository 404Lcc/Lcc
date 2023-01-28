namespace LccModel
{
    /// <summary>
    /// 战斗行动能力
    /// </summary>
    public interface IActionAbility
    {
        public CombatEntity OwnerEntity { get; set; }
        public bool Enable { get; set; }
    }
}