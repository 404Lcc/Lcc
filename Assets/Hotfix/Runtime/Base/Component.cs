namespace LccHotfix
{
    /// <summary>
    /// 游戏框架组件抽象类。
    /// </summary>
    public abstract class Component
    {
        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected virtual void Awake()
        {
            GameEntry.RegisterComponent(this);
        }
    }
}
