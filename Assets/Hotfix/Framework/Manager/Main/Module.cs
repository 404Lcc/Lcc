namespace LccHotfix
{
    /// <summary>
    /// 游戏框架模块抽象类。
    /// </summary>
    public abstract class Module
    {
        /// <summary>
        /// 游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal abstract void Update(float elapseSeconds, float realElapseSeconds);
        internal virtual void LateUpdate() { }
        /// <summary>
        /// 关闭并清理游戏框架模块。
        /// </summary>
        internal abstract void Shutdown();
    }
}