using System;
using UnityEngine;

namespace LccHotfix
{
    public interface IBattleTimeService
    {
        float DeltaTime { get; }
        long NowTicks { get; }
    }

    public sealed class UnityBattleTimeService : IBattleTimeService
    {
        public float DeltaTime => Time.deltaTime;
        public long NowTicks => DateTime.UtcNow.Ticks;
    }

    public static class BattleTime
    {
        public static float GetDeltaTime(LogicWorld world)
        {
            return world?.GetCreationInfo<BattleKernelCreationInfo>()?.BattleTimeService?.DeltaTime ?? Time.deltaTime;
        }

        public static long GetNowTicks(LogicWorld world)
        {
            return world?.GetCreationInfo<BattleKernelCreationInfo>()?.BattleTimeService?.NowTicks ?? DateTime.UtcNow.Ticks;
        }
    }
}
