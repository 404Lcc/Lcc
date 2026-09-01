namespace LccHotfix
{
    /// <summary>
    /// 怪物组件移除时的玩法回调（刷怪计数、掉落、镜头等）。
    /// </summary>
    public interface IMonsterLifecycleSink
    {
        void OnMonsterComponentRemoved(LogicEntity owner, uint tid, bool isSummon, int spawnRuleIndex);
    }
}
