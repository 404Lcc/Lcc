using System;
using UnityEngine;

namespace LccHotfix
{
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
