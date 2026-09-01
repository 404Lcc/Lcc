using System;
using UnityEngine;

namespace LccHotfix
{
    public sealed class UnityBattleTimeService : IBattleTimeService
    {
        public float DeltaTime => Time.deltaTime;
        public long NowTicks => DateTime.UtcNow.Ticks;
    }
}
