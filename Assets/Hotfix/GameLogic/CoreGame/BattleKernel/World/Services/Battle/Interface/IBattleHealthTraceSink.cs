namespace LccHotfix
{
    /// <summary>
    /// 局内血量追踪打点（如 HUD/周期详情）。实现由上层注入。
    /// </summary>
    public interface IBattleHealthTraceSink
    {
        void OnHealthChanged(LogicEntity entity, double hp, double maxHp);
    }
}
