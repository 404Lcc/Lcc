namespace LccHotfix
{
    /// <summary>
    /// Buff标签（mask格式）
    /// 增加新标签需讨论，不值得为单个的buff增加一个标签，除非这个buff需要被批量管理（比如驱散时需要驱散某一类buff）
    /// </summary>
    public static class BuffTag
    {
        public const int Positive = 1 << 0; // 正面
        public const int Negative = 1 << 1; // 负面
        public const int Stun = 1 << 2; // 无法行动
        public const int Control = 1 << 3; // 控制
        public const int DOT = 1 << 4; // 持续伤害
    }
}
