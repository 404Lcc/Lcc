namespace LccHotfix
{
    // 布尔类型免疫效果
    public static class BoolImmune
    {
        public const int InstantKill = 1; // 秒杀
        public const int SlowDown = 2;    // 减速
        public const int Stun = 3;        // 无法行动（眩晕等）
        public const int Pull = 4;        // 牵引
        public const int Teleport = 5;    // 传送
    }

    // 万分比类型免疫效果
    public static class PermyriadImmune
    {
        public const int HitBack = 1;      // 击退抗性
    }
}
