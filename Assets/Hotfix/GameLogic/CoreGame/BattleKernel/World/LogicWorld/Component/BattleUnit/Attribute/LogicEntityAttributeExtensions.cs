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
                // Volume 路径：Scale*Base 乘进基础；血量点数必须在此计入 MaxHp（结算不会再叠 Health Volume）
                // 攻击点数留给 FillFromEntity 叠 Volume.Attack，此处不加，避免双加
                var baseHp = baseProp.UnitHp * (1 + snapshot.ScaleHpBase / 10000f) + snapshot.Health;
                var baseAtk = baseProp.Atk * (1 + snapshot.ScaleAtkBase / 10000f);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Health, fixedHp < 0 ? (float)baseHp : (float)fixedHp);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Attack, fixedAtk < 0 ? (float)baseAtk : fixedAtk);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Defense, fixedDefense < 0 ? baseProp.Def : fixedDefense);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Hit, baseProp.Hit);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Dodge, baseProp.Miss);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Crit, baseProp.Crit);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.CritDamage, baseProp.CritDmg);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.DamageResistance, baseProp.DmgRes);
            }
            else
            {
                FillDefaultTeatAttrCom(comAttributes);
                return comAttributes;
            }

            var addonProp = GetBaseAddonProp(unitCfg);
            if (addonProp != null)
            {
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Crit, addonProp.BaseCritRatio);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.CritResist, addonProp.BaseCritRatioRes);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.CritDamage, addonProp.BaseCritDmgRatio);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.InGameAmplify, addonProp.BaseDmgRatio);
                comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.InGameResistance, addonProp.BaseDmgRatioRes);
                if (BattleLogger.IsDebugEnabled)
                {
                    BattleLogger.LogDebug($"FillAttributes 基础加成属性表: 暴击:{addonProp.BaseCritRatio}, 暴击概率抗:{addonProp.BaseCritRatioRes}, 暴伤加成:{addonProp.BaseCritDmgRatio}, 伤害:{addonProp.BaseDmgRatio}, 伤害抗:{addonProp.BaseDmgRatioRes}");
                }
            }
            else if (BattleLogger.IsDebugEnabled)
            {
                BattleLogger.LogDebug($"FillAttributes 基础加成属性表 == null,  unitCfg:{unitCfg.Base.Id}, unitCfg.BaseAddonPropId={unitCfg.BaseAddonPropId}");
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
                    // 独立池实例，复制基值；禁止与父实体共享同一 IModifyValue
                    var atkValue = parentAtk != null ? parentAtk.DefaultValue : 0;
                    comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Attack, atkValue);
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
            BattleLogger.LogError("FillAttributes 填充属性异常，临时使用缺省属性");
            comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Health, 1000);
            comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Attack, 100);
            comAttributes.SetAttribute<double, MultChangeDouble_ADD>(PropertyFloat.Defense, 50);
        }

        /// <summary>
        /// 根据战斗单位配置和等级获取基础属性组。
        /// </summary>
        private static TBasePropGroup GetBasePropGroup(TBattleUnit unitCfg, int level)
        {
            if (unitCfg == null)
            {
                BattleLogger.LogError("GetBasePropGroup unitCfg == null");
                return null;
            }

            var battleUnitTid = unitCfg.Base.Id;
            var levelBasePropCfg = GetTLevelBaseProp(unitCfg.LevelBaseProp);
            if (levelBasePropCfg == null)
            {
                BattleLogger.LogError($"GetBasePropGroup 等级基础属性表 lvBasePropCfg == null, 战斗单位[{battleUnitTid}] 配置有错误，请策划检查");
                return null;
            }

            if (level >= levelBasePropCfg.Props.Count)
            {
                BattleLogger.LogError($"GetBasePropGroup 等级基础属性表={levelBasePropCfg.Base.Id}, 请策划检查 level[{level}] >= Props.Count[{levelBasePropCfg.Props.Count}] ");
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
                BattleLogger.LogError("GetBaseAddonProp unitCfg == null");
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
                BattleLogger.LogError("GetTLevelBaseProp levelBasePropTid == 0");
                return null;
            }

            var levelBasePropCfg = PbCfg.GetData<TLevelBaseProp>(levelBasePropTid);
            if (levelBasePropCfg == null)
            {
                BattleLogger.LogError($"GetTLevelBaseProp 等级基础属性表 == null, 配置有错误，请策划检查 LevelBaseProp={levelBasePropTid}");
                return null;
            }

            return levelBasePropCfg;
        }
    }
}
