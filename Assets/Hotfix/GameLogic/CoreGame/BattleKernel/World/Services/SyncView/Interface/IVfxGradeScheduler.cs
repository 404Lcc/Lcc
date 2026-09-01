namespace LccHotfix
{
    /// <summary>
    /// 局内特效分级调度：按 path 限流，超量递归降到次级 path。
    /// </summary>
    public interface IVfxGradeScheduler
    {
        /// <summary>
        /// 按分级表决定最终可播 path（不占位）；返回空 path 表示不播。
        /// </summary>
        VfxGradeAcquireResult Acquire(string fxPath);

        /// <summary>
        /// 归还票据对应 path 的在场名额；未 Bind 时安全 no-op。
        /// </summary>
        void Release(VfxGradeTicket ticket);

        /// <summary>
        /// 占位并将票据挂到 FxOne 回收回调；Create 失败时可仍调 Release（未 Bind 则为 no-op）。
        /// </summary>
        void Bind(FxOne fx, VfxGradeTicket ticket);

        /// <summary>
        /// 按配置预载相关 FxCache。
        /// </summary>
        void Preload();

        /// <summary>
        /// 清局时重置在场计数。
        /// </summary>
        void Reset();
    }
}
