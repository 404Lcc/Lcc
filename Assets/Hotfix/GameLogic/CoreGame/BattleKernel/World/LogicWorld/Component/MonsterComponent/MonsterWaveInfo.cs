namespace LccHotfix
{
    /// <summary>
    /// 怪物波次信息结构体
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
