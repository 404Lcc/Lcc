using HotUpdate.Framework.PbCfg;
using PBConfig;

namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 属性初始化和属性配置读取相关扩展。
    /// </summary>
    public static class LogicEntityAttributeExtensions
    {
        /// <summary>
        /// 根据战斗单位配置和等级初始化实体基础属性，支持固定血量、攻击和防御覆盖。
        /// </summary>
        public static AttributesComponent FillAttributes(this LogicEntity entity, TBattleUnit unitCfg, int level, double fixedHp = -1f, float fixedAtk = -1f, float fixedDefense = -1f)
        {
            var comAttributes = entity.hasComAttributes ? entity.comAttributes : entity.AddComAttributes();

            if (unitCfg == null)
            {
                FillDefaultTeatAttrCom(comAttributes);
                return comAttributes;
            }

            if (level > 0)
            {
                level -= 1;
            }

            var baseProp = GetBasePropGroup(unitCfg, level);
            if (baseProp != null)
            {
                var snapshot = new PropertySnapshot();
                snapshot.Add_FromPlayerCategoryVolume(entity);
                var baseAtk = baseProp.Atk * (1 + snapshot.ScaleAtkBase / 10000f);
                comAttributes.SetAttribute<double>(PropertyFloat.Health, new MultChangeDouble_ADD(fixedHp <= 0 ? baseProp.UnitHp : (float)fixedHp));
                comAttributes.SetAttribute<double>(PropertyFloat.Attack, new MultChangeDouble_ADD(fixedAtk < 0 ? (float)baseAtk : fixedAtk));
                comAttributes.SetAttribute<double>(PropertyFloat.Defense, new MultChangeDouble_ADD(fixedDefense < 0 ? baseProp.Def : fixedDefense));
                comAttributes.SetAttribute<double>(PropertyFloat.Hit, new MultChangeDouble_ADD(baseProp.Hit));
                comAttributes.SetAttribute<double>(PropertyFloat.Dodge, new MultChangeDouble_ADD(baseProp.Miss));
            }
            else
            {
                FillDefaultTeatAttrCom(comAttributes);
                return comAttributes;
            }

            var addonProp = GetBaseAddonProp(unitCfg);
            if (addonProp != null)
            {
                comAttributes.SetAttribute<double>(PropertyFloat.Crit, new MultChangeDouble_ADD(addonProp.BaseCritRatio));
                comAttributes.SetAttribute<double>(PropertyFloat.CritResist, new MultChangeDouble_ADD(addonProp.BaseCritRatioRes));
                comAttributes.SetAttribute<double>(PropertyFloat.CritDamage, new MultChangeDouble_ADD(addonProp.BaseCritDmgRatio));
                comAttributes.SetAttribute<double>(PropertyFloat.InGameAmplify, new MultChangeDouble_ADD(addonProp.BaseDmgRatio));
                comAttributes.SetAttribute<double>(PropertyFloat.InGameResistance, new MultChangeDouble_ADD(addonProp.BaseDmgRatioRes));
                if (BattleLog.IsDebugEnabled)
                {
                    BattleLog.Debug($"FillAttributes 基础加成属性表: 暴击:{addonProp.BaseCritRatio}, 暴击概率抗:{addonProp.BaseCritRatioRes}, 暴伤加成:{addonProp.BaseCritDmgRatio}, 伤害:{addonProp.BaseDmgRatio}, 伤害抗:{addonProp.BaseDmgRatioRes}");
                }
            }
            else if (BattleLog.IsDebugEnabled)
            {
                BattleLog.Debug($"FillAttributes 基础加成属性表 == null,  unitCfg:{unitCfg.Base.Id}, unitCfg.BaseAddonPropId={unitCfg.BaseAddonPropId}");
            }

            return comAttributes;
        }

        /// <summary>
        /// 根据父实体属性初始化当前实体属性，当前仅继承攻击力。
        /// </summary>
        public static AttributesComponent FillAttributesByParent(this LogicEntity entity, LogicEntity parent)
        {
            var comAttributes = entity.hasComAttributes ? entity.comAttributes : entity.AddComAttributes();

            if (parent != null && parent.hasComAttributes)
            {
                var parentAttributes = parent.comAttributes;
                if (parentAttributes != null)
                {
                    var parentAtk = parentAttributes.GetAttribute<double>(PropertyFloat.Attack);
                    comAttributes.SetAttribute<double>(PropertyFloat.Attack, parentAtk);
                    return comAttributes;
                }
            }

            FillDefaultTeatAttrCom(comAttributes);
            return comAttributes;
        }

        /// <summary>
        /// 填充兜底测试属性，避免配置异常时实体完全无属性。
        /// </summary>
        private static void FillDefaultTeatAttrCom(AttributesComponent comAttributes)
        {
            BattleLog.Error("FillAttributes 填充属性异常，临时使用缺省属性");
            comAttributes.SetAttribute<double>(PropertyFloat.Health, new MultChangeDouble_ADD(1000));
            comAttributes.SetAttribute<double>(PropertyFloat.Attack, new MultChangeDouble_ADD(100));
            comAttributes.SetAttribute<double>(PropertyFloat.Defense, new MultChangeDouble_ADD(50));
        }

        /// <summary>
        /// 根据战斗单位配置和等级获取基础属性组。
        /// </summary>
        private static TBasePropGroup GetBasePropGroup(TBattleUnit unitCfg, int level)
        {
            if (unitCfg == null)
            {
                BattleLog.Error("GetBasePropGroup unitCfg == null");
                return null;
            }

            var battleUnitTid = unitCfg.Base.Id;
            var levelBasePropCfg = GetTLevelBaseProp(unitCfg.LevelBaseProp);
            if (levelBasePropCfg == null)
            {
                BattleLog.Error($"GetBasePropGroup 等级基础属性表 lvBasePropCfg == null, 战斗单位[{battleUnitTid}] 配置有错误，请策划检查");
                return null;
            }

            if (level >= levelBasePropCfg.Props.Count)
            {
                BattleLog.Error($"GetBasePropGroup 等级基础属性表={levelBasePropCfg.Base.Id}, 请策划检查 level[{level}] >= Props.Count[{levelBasePropCfg.Props.Count}] ");
                return null;
            }

            return levelBasePropCfg.Props[level];
        }

        /// <summary>
        /// 获取战斗单位通用基础附加属性配置。
        /// </summary>
        private static TBaseAddonProp GetBaseAddonProp(TBattleUnit unitCfg)
        {
            if (unitCfg == null)
            {
                BattleLog.Error("GetBaseAddonProp unitCfg == null");
                return null;
            }

            if (unitCfg.BaseAddonPropId == 0)
            {
                return null;
            }

            return PbCfg.GetData<TBaseAddonProp>(unitCfg.BaseAddonPropId);
        }

        /// <summary>
        /// 根据等级属性配置 ID 获取等级基础属性配置。
        /// </summary>
        private static TLevelBaseProp GetTLevelBaseProp(uint levelBasePropTid)
        {
            if (levelBasePropTid == 0)
            {
                BattleLog.Error("GetTLevelBaseProp levelBasePropTid == 0");
                return null;
            }

            var levelBasePropCfg = PbCfg.GetData<TLevelBaseProp>(levelBasePropTid);
            if (levelBasePropCfg == null)
            {
                BattleLog.Error($"GetTLevelBaseProp 等级基础属性表 == null, 配置有错误，请策划检查 LevelBaseProp={levelBasePropTid}");
                return null;
            }

            return levelBasePropCfg;
        }
    }
}
