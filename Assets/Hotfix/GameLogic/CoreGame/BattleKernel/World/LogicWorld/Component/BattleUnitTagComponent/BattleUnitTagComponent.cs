using PBConfig;

namespace LccHotfix
{
    //按照各种维度罗列的实体可能归属的标签集
    //不用关注名字、是否有冲突等。 这里是不过脑子的标记集合，出生就标好中间不做修改
    public struct BattleUnitTag
    {
        public int? PlayerIndex;
        public int? FighterId;          //局内战斗实体表ID
        public int? BattleUnitId;      //战斗单位属性表ID
        public int? GameWeaponId;      //局内武器表ID
        
        public TCampType? Camp;         //我方单位阵营
        public TElementType? Element;   //我方单位元素
        public THeroAttackType? AttackType; // 我方攻击类型
        public TBattleUnitType? BattleUnitType; //战斗单位类型枚举

        public TEnemyAttackType? EnemyAttackType;       //敌方单位攻击
        public TEnemyStrengthType? EnemyStrengthType;   //敌方单位强度
        public TEnemyRaceType? EnemyRaceType;           //敌方单位种族

        public int Mask
        {
            get
            {
                int mask = 0;
                int bitPosition = 0;

                // 每个字段占用1位，表示该字段是否有有效值
                // 对于可空类型，检查HasValue
                mask |= (PlayerIndex.HasValue ? 1 : 0) << bitPosition++;
                mask |= (FighterId.HasValue ? 1 : 0) << bitPosition++;
                mask |= (BattleUnitId.HasValue ? 1 : 0) << bitPosition++;
                mask |= (GameWeaponId.HasValue ? 1 : 0) << bitPosition++;
                mask |= (Camp.HasValue ? 1 : 0) << bitPosition++;
                mask |= (Element.HasValue ? 1 : 0) << bitPosition++;
                mask |= (AttackType.HasValue ? 1 : 0) << bitPosition++;
                mask |= (BattleUnitType.HasValue ? 1 : 0) << bitPosition++;
                mask |= (EnemyAttackType.HasValue ? 1 : 0) << bitPosition++;
                mask |= (EnemyStrengthType.HasValue ? 1 : 0) << bitPosition++;
                mask |= (EnemyRaceType.HasValue ? 1 : 0) << bitPosition++;

                return mask;
            }
        }
        
        // 检查是否包含要求的字段
        public bool Contains(params int[] requiredBits)
        {
            foreach (int bit in requiredBits)
            {
                if ((Mask & (1 << bit)) == 0)
                    return false;
            }
            return true;
        }
    }

    //带有项目特殊性的身份标签，暂时属于特化组件，将来可能会规整复用
    public class BattleUnitTagComponent : LogicComponent
    {
        public BattleUnitTag Tag { get; private set; }

        public void Init(BattleUnitTag tag)
        {
            Tag = tag;
        }
    }

    public partial class LogicEntity
    {
        public BattleUnitTagComponent comBattleUnitTag
        {
            get { return (BattleUnitTagComponent)GetComponent(LogicComponentsLookup.ComBattleUnitTag); }
        }

        public bool hasComBattleUnitTag
        {
            get { return HasComponent(LogicComponentsLookup.ComBattleUnitTag); }
        }

        public void AddComBattleUnitTag(BattleUnitTag tag)
        {
            var index = LogicComponentsLookup.ComBattleUnitTag;
            var component = (BattleUnitTagComponent)CreateComponent(index, typeof(BattleUnitTagComponent));
            component.Init(tag);
            AddComponent(index, component);
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComBattleUnitTagIndex = new(typeof(BattleUnitTagComponent));
        public static int ComBattleUnitTag => _ComBattleUnitTagIndex.Index;
    }
}
