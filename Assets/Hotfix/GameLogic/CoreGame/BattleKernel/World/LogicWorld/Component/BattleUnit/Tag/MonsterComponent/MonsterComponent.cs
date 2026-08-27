namespace LccHotfix
{
    // 怪物身份组件
    public class MonsterComponent : LogicComponent
    {
        // 怪物的配置
        public uint Tid { get; private set; }
        public int Level { get; private set; } // 怪物等级

        public void Init(uint tid, int level)
        {
            Tid = tid;
            Level = level;
        }

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();

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
