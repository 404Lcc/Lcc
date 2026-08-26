using HotUpdate.Framework.PbCfg;
using PBConfig;
using UnityEngine;

namespace LccHotfix
{
    public static class CustomNodeSourceExtensions
    {
    #region 单位来源与子物体来源


        /// <summary>
        /// 尝试获取当前逻辑生成信息中的整合单位来源。
        /// </summary>
        public static bool GetSumUnitSource(this CustomNode self, out UnitSource skillUnitSource)
        {
            if (self.GenInfo is IHasSumUnitSource hasSumUnitSource)
            {
                skillUnitSource = hasSumUnitSource.SumUnitSource;
                return true;
            }

            skillUnitSource = default;
            return false;
        }

        /// <summary>
        /// 获取当前全部标签身份整合后的参战单位来源信息。
        /// </summary>
        public static UnitSource GetSumUnitSource(this CustomNode self, bool autoCreate = false)
        {
            if (self.GenInfo is IHasSumUnitSource hasSumUnitSource)
            {
                return hasSumUnitSource.SumUnitSource;
            }

            if (autoCreate)
            {
                return self.CreateTempSumUnitSource();
            }

            return default;
        }

        /// <summary>
        /// 基于当前拥有者战斗实体临时创建单位来源信息。
        /// </summary>
        public static UnitSource CreateTempSumUnitSource(this CustomNode self)
        {
            var world = self.GetLogicWorld();
            if (world == null)
            {
                self.LogError($"node.GetUnitSource world == null");
                return default;
            }

            //标准先用GetGenInfo获取
            var attackerID = self.GetOwnerFighterEntityID();

            var e_attacker = world.GetEntityWithComID(attackerID);
            if (e_attacker == null)
            {
                self.LogError($"node.GetUnitSource e_attacker == null, attackerID={attackerID}");
                return default;
            }

            return new UnitSource(e_attacker);
        }

        /// <summary>
        /// 尝试获取当前逻辑生成信息中的子物体来源。
        /// </summary>
        public static bool GetSubobjectSource(this CustomNode self, out SubobjectSource sbjSource)
        {
            if (self.GenInfo is IHasSubobjectSource hasSubobj)
            {
                sbjSource = hasSubobj.SubobjSource.GetValueOrDefault();
                return true;
            }

            sbjSource = default;
            return false;
        }
        
        /// <summary>
        /// 尝试获取当前逻辑中的伤害类型。
        /// </summary>
        public static bool GetDamageType(this CustomNode self, out TElementType damageType)
        {
            if (self.GenInfo is IHasDamageType hasDamageType)
            {
                damageType = hasDamageType.DamageType;
                return true;
            }

            damageType = default;
            return false;
        }

        /// <summary>
        /// 读取当前逻辑 GenInfo 上的元素类型；没有 IHasDamageType 时视为 EetAll。
        /// </summary>
        public static TElementType ResolveElementType(this CustomNode self)
        {
            if (self != null && self.GetDamageType(out var damageType))
                return damageType;
            return TElementType.EetAll;
        }

    #endregion

    #region 单位属性与特性


        /// <summary>
        /// 累加当前整合单位来源属性中的局内增幅值。
        /// </summary>
        public static void AddSumProperty_InGameAmplify(this CustomNode self, float inGameAmplify)
        {
            var theGenInfo = self.GetGenInfo<IHasSumUnitSource>();
            if (theGenInfo == null)
            {
                self.LogError("AddSumProperty_InGameAmplify theGenInfo == null");
                return;
            }

            theGenInfo.SumUnitSource.Properties.InGameAmplify += inGameAmplify;
        }

        /// <summary>
        /// 根据拥有者韧性计算 Buff 持续时间倍率，并限制在合理区间。
        /// </summary>
        public static float GetTenacityBuffDurationRate(this CustomNode self)
        {
            var ownerEntity = self.GetOwnerEntity();
            var ownerUnitSource = new UnitSource(ownerEntity);
            var tenacity = ownerUnitSource.Properties.Tenacity;
            if (tenacity == 0f)
            {
                return 1f;
            }

            var rate = (float)(10000f - tenacity) / 10000f;
            return Mathf.Clamp(rate, 0.2f, 2f);
        }

        /// <summary>
        /// 获取当前全部标签身份整合后的属性快照。
        /// </summary>
        public static PropertySnapshot GetSumProperties(this CustomNode self)
        {
            var unitSource = self.GetSumUnitSource();
            return unitSource.Properties;
        }

        /// <summary>
        /// 获取当前全部标签身份整合后的特性数据。
        /// </summary>
        public static UnitSkillFeature GetSumFeature(this CustomNode self)
        {
            var unitSource = self.GetSumUnitSource();
            return unitSource.Properties.Feature;
        }

    #endregion
    }
}
