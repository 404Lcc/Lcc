namespace LccHotfix
{
    public partial class LogicWorld
    {
        /// <summary>
        /// 本局 IViewWrapper 包装对象池，随 World 一起释放。
        /// </summary>
        public ViewWrapperPool ViewWrapperPool { get; } = new ViewWrapperPool();
    }
}
