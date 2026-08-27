using System.Collections.Generic;
using PBConfig;

namespace LccHotfix
{
    //人类、怪物 都可以有一个抽象的归属 Player
    public interface IPlayerInfo : IBattlePlayerInfo
    {
        public int PlayerIndex { get; }
        public CategoryVolumeInfo VolumePlayer { get; }
        public bool IsLocalPlayer { get; }
    }

    public partial class InGamePlayerInfo : IPlayerInfo, IBuffDurationModifier
    {
        public long PlayerUid { get; private set; } = 0;
        public int PlayerIndex { get; private set; } = 0;
        public bool IsLocalPlayer { get; private set; } = false;
        public int InGameLevel { get; set; }

        // 局内归属阵营；创建实体时 ReplaceComFaction 优先读此值
        public EFaction PlayerFaction { get; set; } = EFaction.Invalid;

        //开局信息
        public GameHeroInfo Hero { get; set; } = new();

        public PlayerFeaturesContext FeaturesContext = new();


        public float DebuffDurationAddRate => FeaturesContext.PointDebuffDuration;

        //属性黑版
        public CategoryVolumeInfo VolumePlayer { get; private set; }
        private readonly Dictionary<ECategoryVolume, object> _categoryVolumes = new();

        public InGamePlayerInfo(int idx, long uid, bool isLocalPlayer, int lvl)
        {
            PlayerUid = uid;
            PlayerIndex = idx;
            IsLocalPlayer = isLocalPlayer;
            InGameLevel = lvl;

            //玩家全局特性：
            VolumePlayer = new();

            //玩家细分特性：所有分类字典
            _categoryVolumes[ECategoryVolume.FighterTid] = new Dictionary<int, CategoryVolumeInfo>();
            _categoryVolumes[ECategoryVolume.BattleUnit] = new Dictionary<int, CategoryVolumeInfo>();
            _categoryVolumes[ECategoryVolume.SubobjectTid] = new Dictionary<int, CategoryVolumeInfo>();
            _categoryVolumes[ECategoryVolume.Camp] = new Dictionary<TCampType, CategoryVolumeInfo>();
            _categoryVolumes[ECategoryVolume.Element] = new Dictionary<TElementType, CategoryVolumeInfo>();
            _categoryVolumes[ECategoryVolume.UnitType] = new Dictionary<TBattleUnitType, CategoryVolumeInfo>();
            _categoryVolumes[ECategoryVolume.LevelType] = new Dictionary<TLevelType, CategoryVolumeInfo>();
            _categoryVolumes[ECategoryVolume.Skill] = new Dictionary<int, CategoryVolumeInfo>();
            _categoryVolumes[ECategoryVolume.EnemyStrength] = new Dictionary<TEnemyStrengthType, CategoryVolumeInfo>();
            _categoryVolumes[ECategoryVolume.AttackType] = new Dictionary<THeroAttackType, CategoryVolumeInfo>();

        }

        protected TVolume GetCategory<TKey, TVolume>(ECategoryVolume volumeType, TKey key, bool autoCreate = false) where TKey : notnull where TVolume : CategoryVolumeInfo, new()
        {
            if (_categoryVolumes.TryGetValue(volumeType, out var dictObj) && dictObj is Dictionary<TKey, CategoryVolumeInfo> dict)
            {
                if (dict.TryGetValue(key, out var category))
                    return category as TVolume;

                if (autoCreate)
                {
                    category = new TVolume();
                    dict[key] = category;
                    return category as TVolume;
                }
            }

            return null;
        }

        // 按局内战斗实体表TID取分类属性，常用于某个战斗实体模板的属性/主技能改写
        public CategoryVolumeInfo_Fighter GetVolume_FighterTid(int fighterTid, bool autoCreate = false) => GetCategory<int, CategoryVolumeInfo_Fighter>(ECategoryVolume.FighterTid, fighterTid, autoCreate);

        // 按子物体TID取分类属性，常用于子弹/子物体属性与表现覆盖
        public CategoryVolumeInfo_Subobject GetVolume_SubobjectTid(int tid, bool autoCreate = false) => GetCategory<int, CategoryVolumeInfo_Subobject>(ECategoryVolume.SubobjectTid, tid, autoCreate);

        // 按战斗单位属性表TID取分类属性，作用于某类单位属性模板
        public CategoryVolumeInfo GetVolume_BattleUnit(int battleUnitId, bool autoCreate = false) => GetCategory<int, CategoryVolumeInfo>(ECategoryVolume.BattleUnit, battleUnitId, autoCreate);

        // 按阵营取分类属性
        public CategoryVolumeInfo GetVolume_Camp(TCampType camp, bool autoCreate = false) => GetCategory<TCampType, CategoryVolumeInfo>(ECategoryVolume.Camp, camp, autoCreate);

        // 按元素取分类属性
        public CategoryVolumeInfo GetVolume_Element(TElementType element, bool autoCreate = false) => GetCategory<TElementType, CategoryVolumeInfo>(ECategoryVolume.Element, element, autoCreate);

        // 按攻击类型取分类属性
        public CategoryVolumeInfo GetVolume_AttackType(THeroAttackType attackType, bool autoCreate = false) => GetCategory<THeroAttackType, CategoryVolumeInfo>(ECategoryVolume.AttackType, attackType, autoCreate);

        // 按战斗单位类型取分类属性
        public CategoryVolumeInfo GetVolume_UnitType(TBattleUnitType unitType, bool autoCreate = false) => GetCategory<TBattleUnitType, CategoryVolumeInfo>(ECategoryVolume.UnitType, unitType, autoCreate);

        // 按关卡类型取分类属性
        public CategoryVolumeInfo GetVolume_LevelType(TLevelType levelType, bool autoCreate = false) => GetCategory<TLevelType, CategoryVolumeInfo>(ECategoryVolume.LevelType, levelType, autoCreate);

        // 按敌人强度取分类属性
        public CategoryVolumeInfo GetVolume_EnemyStrength(TEnemyStrengthType enemyStrength, bool autoCreate = false) => GetCategory<TEnemyStrengthType, CategoryVolumeInfo>(ECategoryVolume.EnemyStrength, enemyStrength, autoCreate);

        // 按技能TID取分类属性，作用于具体技能模板
        public CategoryVolumeInfo GetVolume_Skill(int skillTid, bool autoCreate = false) => GetCategory<int, CategoryVolumeInfo>(ECategoryVolume.Skill, skillTid, autoCreate);

        public HeroInfo GetHeroInfo(int index)
        {
            if (Hero?.HeroInfos == null || index < 0 || index >= Hero.HeroInfos.Count)
            {
                return null;
            }

            return Hero.HeroInfos[index];
        }

        public HeroInfo GetCaptainHeroInfo()
        {
            return GetHeroInfo(0);
        }
        //继续增补
    }
}