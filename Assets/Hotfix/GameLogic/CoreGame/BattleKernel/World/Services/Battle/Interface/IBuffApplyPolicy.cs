namespace LccHotfix
{
    /// <summary>
    /// Buff 上身前的玩法策略（如战术挂件控免）。
    /// 返回 true 表示已拦截并消费免疫，Buff 不应再创建。
    /// </summary>
    public interface IBuffApplyPolicy
    {
        bool TryBlockBuffApply(LogicEntity target, BuffGenInfo genInfo);
    }
}
