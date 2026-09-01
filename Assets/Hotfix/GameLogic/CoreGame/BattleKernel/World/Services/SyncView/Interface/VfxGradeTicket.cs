namespace LccHotfix
{
    /// <summary>
    /// 分级票据：Bind 时按 Path 占位，播毕 OnReleased 或显式 Release 归还。
    /// </summary>
    public readonly struct VfxGradeTicket
    {
        // 实际占用名额的 path；未计入分级表时为空
        public readonly string Path;
        // Bind 时是否占位，并需在播毕/失败时归还
        public readonly bool NeedsRelease;

        public VfxGradeTicket(string path, bool needsRelease)
        {
            Path = path;
            NeedsRelease = needsRelease;
        }

        /// <summary>
        /// 不占位的空票据。
        /// </summary>
        public static VfxGradeTicket None => default;

        /// <summary>
        /// 表内 path：Bind 时占一个在场名额。
        /// </summary>
        public static VfxGradeTicket ForCounted(string path)
        {
            return new VfxGradeTicket(path, true);
        }
    }
}
