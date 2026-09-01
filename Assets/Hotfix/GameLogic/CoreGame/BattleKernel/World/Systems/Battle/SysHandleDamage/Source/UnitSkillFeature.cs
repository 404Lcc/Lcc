using System;

namespace LccHotfix
{
    // 只加给某个分类个体的特性。
    [Serializable]
    public struct UnitSkillFeature
    {
        // 基础属性增强
        public int SubBulletCollisionCountAdd;           // 次级子弹碰撞次数增加（适用于：全子弹穿透）
        public int PointExtraAttackTimes;                // 点数修正额外攻击次数

        // 分裂系统
        public float SkillCdReducePercent;               // 技能CD减少百分比
        public float SkillCdTimeSecondAdd;               // 技能CD增量S

        public void Add(in UnitSkillFeature other)
        {
            SubBulletCollisionCountAdd += other.SubBulletCollisionCountAdd;
            PointExtraAttackTimes += other.PointExtraAttackTimes;


            SkillCdReducePercent += other.SkillCdReducePercent;
            SkillCdTimeSecondAdd += other.SkillCdTimeSecondAdd;
        }

        public static UnitSkillFeature operator +(UnitSkillFeature a, UnitSkillFeature b)
        {
            UnitSkillFeature result = a;
            result.Add(b);
            return result;
        }
    }
}
