namespace LccHotfix
{
    public static class CustomNodeDebugExtensions
    {
    #region 调试辅助


        /// <summary>
        /// 判断当前是否为开发日志模式。
        /// </summary>
        public static bool IsDev(this CustomNode self)
        {
            return BattleLogger.IsDebugEnabled;
        }

    #endregion
    }
}
