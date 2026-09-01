using PBConfig;

namespace LccHotfix
{
    /// <summary>
    /// 战斗单位属性相关配置表读取扩展（Kernel 层，供属性填充等复用）。
    /// </summary>
    public static class BattleUnitConfigExtensions
    {
        public static TBasePropGroup GetBasePropGroup(this TBattleUnit unitCfg, int level)
        {
            if (unitCfg == null)
            {
                BattleLogger.LogError("GetBasePropGroup unitCfg == null");
                return null;
            }

            var battleUnitTID = unitCfg.Base.Id;
            var lvBasePropCfg = PbCfg.GetData<TLevelBaseProp>(unitCfg.LevelBaseProp);
            if (lvBasePropCfg == null)
            {
                BattleLogger.LogError($"GetBasePropGroup 等级基础属性表 lvBasePropCfg == null, 战斗单位[{battleUnitTID}] 配置有错误，请策划检查");
                return null;
            }

            return lvBasePropCfg.GetBasePropGroup(level);
        }

        public static TBaseAddonProp GetBaseAddonProp(this TBattleUnit unitCfg)
        {
            if (unitCfg == null)
            {
                BattleLogger.LogError("GetBaseAddonProp unitCfg == null");
                return null;
            }

            if (unitCfg.BaseAddonPropId == 0)
            {
                return null;
            }

            return PbCfg.GetData<TBaseAddonProp>(unitCfg.BaseAddonPropId);
        }

        public static TBasePropGroup GetBasePropGroup(this TLevelBaseProp lvBasePropCfg, int level)
        {
            if (lvBasePropCfg == null)
            {
                BattleLogger.LogError("GetBasePropGroup 等级基础属性表 == null");
                return null;
            }

            if (level >= lvBasePropCfg.Props.Count)
            {
                BattleLogger.LogError($"GetBasePropGroup 等级基础属性表={lvBasePropCfg.Base.Id}, 请策划检查 level[{level}] >= Props.Count[{lvBasePropCfg.Props.Count}] ");
                return null;
            }

            return lvBasePropCfg.Props[level];
        }
    }
}