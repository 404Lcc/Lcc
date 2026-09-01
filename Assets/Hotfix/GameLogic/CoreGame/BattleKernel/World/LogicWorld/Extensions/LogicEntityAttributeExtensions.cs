using PBConfig;

namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 属性初始化和属性配置读取相关扩展。
    /// </summary>
    public static class LogicEntityAttributeExtensions
    {
        /// <summary>
        /// 获取定点数属性值，读取失败时返回指定错误值。
        /// </summary>
        public static FixPoint GetAttributeFixPoint(this LogicEntity e, int key, FixPoint errorValue = default(FixPoint))
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttributeFixPoint  !HasAttributes");
                return errorValue;
            }

            var rv = errorValue;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return errorValue;
        }

        /// <summary>
        /// 获取 float 属性值，读取失败时返回指定错误值。
        /// </summary>
        public static float GetAttributeFloat(this LogicEntity e, int key, float errorValue = 0)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttribute  !HasAttributes");
                return errorValue;
            }

            var rv = errorValue;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return errorValue;
        }

        /// <summary>
        /// 获取 double 属性值，读取失败时返回指定错误值。
        /// </summary>
        public static double GetAttributeDouble(this LogicEntity e, int key, double errorValue = 0)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttribute  !HasAttributes");
                return errorValue;
            }

            var rv = errorValue;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return errorValue;
        }

        /// <summary>
        /// 获取 int 属性值，读取失败时返回指定错误值。
        /// </summary>
        public static int GetAttributeInt(this LogicEntity e, int key, int errorValue = 0)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttribute  !HasAttributes");
                return errorValue;
            }

            var rv = errorValue;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return errorValue;
        }

        /// <summary>
        /// 获取 bool 属性值，读取失败时返回指定错误值。
        /// </summary>
        public static bool GetAttributeBool(this LogicEntity e, int key, bool errorValue = true)
        {
            var rv = errorValue;
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("e.GetAttribute  !HasAttributes");
                return rv;
            }

            if (!e.hasComAttributes)
                return rv;
            if (e.comAttributes.TryGetValue(key, ref rv))
                return rv;
            else
                return rv;
        }

        /// <summary>
        /// 对 float 属性添加指定 flag 的修改值。
        /// </summary>
        public static void ModifyAttribute(this LogicEntity e, int key, float value, int flag)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("Actor.SetAttribute  !HasAttributes");
                return;
            }

            e.comAttributes.Modify(key, value, flag);
        }

        /// <summary>
        /// 对 int 属性添加指定 flag 的修改值。
        /// </summary>
        public static void ModifyAttribute(this LogicEntity e, int key, int value, int flag)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("Actor.SetAttribute  !HasAttributes");
                return;
            }

            e.comAttributes.Modify(key, value, flag);
        }

        /// <summary>
        /// 对 bool 属性添加指定 flag 的修改值。
        /// </summary>
        public static void ModifyAttribute(this LogicEntity e, int key, bool value, int flag)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("Actor.SetAttribute  !HasAttributes");
                return;
            }

            e.comAttributes.Modify(key, value, flag);
        }

        /// <summary>
        /// 移除指定类型属性上对应 flag 的修改值。
        /// </summary>
        public static void RemoveModify<T>(this LogicEntity e, int key, int flag)
        {
            if (e == null || !e.hasComAttributes)
            {
                BattleLogger.LogError("Actor.RemoveAttribute  !HasAttributes");
                return;
            }

            e.comAttributes.RemoveModify<T>(key, flag);
        }

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

            var baseProp = unitCfg.GetBasePropGroup(level);
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

            var addonProp = unitCfg.GetBaseAddonProp();
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
        /// 判断实体是否处于隐身且未被反隐揭示状态。
        /// </summary>
        public static bool IsCloaked(this LogicEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (!entity.hasComAttributes)
            {
                return false;
            }

            return entity.GetAttributeBool(AttributeBool.Cloaked, false) && !entity.GetAttributeBool(AttributeBool.CloakRevealed, false);
        }

        /// <summary>
        /// 判断实体是否处于飞行状态
        /// </summary>
        public static bool IsFlying(this LogicEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (!entity.hasComAttributes)
            {
                return false;
            }

            return entity.GetAttributeBool(AttributeBool.Flying, false);
        }

        /// <summary>
        /// 判断实体是否允许参与碰撞检测。
        /// </summary>
        public static bool CanCollider(this LogicEntity entity)
        {
            if (entity.hasComAttributes)
            {
                var comAttributes = entity.comAttributes;
                if (!comAttributes.GetValue<bool>(AttributeBool.CanCollision, false))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断实体是否允许被选为目标。
        /// </summary>
        public static bool CanBeTarget(this LogicEntity entity, bool AA = false)
        {
            if (entity.hasComAttributes)
            {
                var comAttributes = entity.comAttributes;
                if (!comAttributes.GetValue<bool>(AttributeBool.CanBeTargeted, false))
                {
                    return false;
                }
                if (!AA && comAttributes.GetValue<bool>(AttributeBool.Flying, false))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断实体是否允许受到伤害。
        /// </summary>
        public static bool CanBeHurt(this LogicEntity entity)
        {
            if (entity.hasComAttributes)
            {
                var comAttributes = entity.comAttributes;
                if (!comAttributes.GetValue<bool>(AttributeBool.CanBeHurted, false))
                {
                    return false;
                }
            }

            return true;
        }
    }
}