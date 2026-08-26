
namespace LccHotfix
{
    // 怪物身份组件
    public class MonsterComponent : LogicComponent
    {
        // 怪物的配置
        public uint Tid { get; private set; }
        public int Level { get; private set; } // 怪物等级
        public bool IsSummon { get; private set; }
        public MonsterWaveInfo WaveInfo { get; private set; }
        public int SpawnRuleIndex => WaveInfo.SpawnRuleIndex; // 刷怪规则
        public int WaveIndex => WaveInfo.WaveIndex; // 波次
        public int WaveInnerCount => WaveInfo.WaveInnerCount; // 波次内怪物总数（我是第几个）
        public int WaveInnerIndex => WaveInfo.WaveInnerIndex; // 波次内怪物索引
        public float WaveTimeSpacing => WaveInfo.WaveTimeSpacing; // 波次内怪物生成时间偏移

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
            Main.ValueEventService.Dispatch(new EvtMonsterDead()
            {
                Tid = Tid,
                Position = _owner.position,
                SpawnRuleIndex = SpawnRuleIndex,
                HolderEntityId = _owner.hasComHolder ? _owner.comHolder.HolderEntityID : 0,
                IsSummon = IsSummon,
            });
            
            base.DisposeOnRemove();
            Tid = 0;
            IsSummon = false;
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
