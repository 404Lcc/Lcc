namespace LccHotfix
{
    // 怪物身份组件
    public class MonsterComponent : LogicComponent
    {
        public uint Tid { get; private set; }
        public int Level { get; private set; }
        public bool IsSummon { get; private set; }
        public MonsterWaveInfo WaveInfo { get; private set; }
        public int SpawnRuleIndex => WaveInfo.SpawnRuleIndex;
        public int WaveIndex => WaveInfo.WaveIndex;
        public int WaveInnerCount => WaveInfo.WaveInnerCount;
        public int WaveInnerIndex => WaveInfo.WaveInnerIndex;
        public float WaveTimeSpacing => WaveInfo.WaveTimeSpacing;

        public void Init(uint tid, int level)
        {
            Tid = tid;
            Level = level;
        }

        public void InitWaveInfo(MonsterWaveInfo waveInfo)
        {
            WaveInfo = waveInfo;
        }

        public void SetIsSummon(bool isSummon)
        {
            IsSummon = isSummon;
        }

        public override void DisposeOnRemove()
        {
            _owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.MonsterLifecycleSink
                ?.OnMonsterComponentRemoved(_owner, Tid, IsSummon, SpawnRuleIndex);

            base.DisposeOnRemove();
            Tid = 0;
            IsSummon = false;
            WaveInfo = default;
        }
    }

    public partial class LogicEntity
    {
        public MonsterComponent comMonster
        {
            get { return (MonsterComponent)GetComponent(LogicComponentsLookup.ComMonster); }
        }

        public bool hasComMonster
        {
            get { return HasComponent(LogicComponentsLookup.ComMonster); }
        }

        public void AddComMonster(uint tid, int level)
        {
            var index = LogicComponentsLookup.ComMonster;
            var component = (MonsterComponent)CreateComponent(index, typeof(MonsterComponent));
            component.Init(tid, level);
            AddComponent(index, component);
        }

        public void RemoveComMonster()
        {
            RemoveComponent(LogicComponentsLookup.ComMonster);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComMonsterIndex = new(typeof(MonsterComponent));
        public static int ComMonster => _ComMonsterIndex.Index;
    }
}
