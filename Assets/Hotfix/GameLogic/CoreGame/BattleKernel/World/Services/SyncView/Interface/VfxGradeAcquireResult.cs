namespace LccHotfix
{
    /// <summary>
    /// 分级 Acquire 结果：最终可播 path（可空=不播）与归还票据。
    /// </summary>
    public readonly struct VfxGradeAcquireResult
    {
        // 最终播出 path；空表示跳过播放
        public readonly string Path;
        public readonly VfxGradeTicket Ticket;

        public VfxGradeAcquireResult(string path, VfxGradeTicket ticket)
        {
            Path = path;
            Ticket = ticket;
        }

        /// <summary>
        /// 无调度器时的透传：原 path、不占位。
        /// </summary>
        public static VfxGradeAcquireResult PassThrough(string path)
        {
            return new VfxGradeAcquireResult(path, VfxGradeTicket.None);
        }
    }
}
