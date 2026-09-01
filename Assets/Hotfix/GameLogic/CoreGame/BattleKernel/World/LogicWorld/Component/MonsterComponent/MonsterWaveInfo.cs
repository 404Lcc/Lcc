namespace LccHotfix
{
    /// <summary>
    /// 怪物波次信息（刷怪规则/波次索引）。定义在 Kernel，供 MonsterComponent 持有。
    /// </summary>
    public struct MonsterWaveInfo
    {
        public int SpawnRuleIndex;
        public int WaveIndex;
        public int WaveInnerCount;
        public int WaveInnerIndex;
        public float WaveTimeSpacing;

        public MonsterWaveInfo(int spawnRuleIndex = -1,
            int waveIndex = -1,
            int waveInnerCount = 0,
            int waveInnerIndex = 0,
            float waveTimeSpacing = 0f)
        {
            SpawnRuleIndex = spawnRuleIndex;
            WaveIndex = waveIndex;
            WaveInnerCount = waveInnerCount;
            WaveInnerIndex = waveInnerIndex;
            WaveTimeSpacing = waveTimeSpacing;
        }
    }
}
