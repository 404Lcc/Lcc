using System;

namespace LccHotfix
{
    // 只加给某个分类个体的特性。
    [Serializable]
    public partial struct UnitSkillFeature
    {
        // 基础属性增强
        public int BulletCountAdd;                       // 子弹数量增加
        public int CollisionCountAdd;                    // 碰撞次数增加（适用于：子弹穿透、电磁炮弹射等）
        public int SubBulletCollisionCountAdd;           // 次级子弹碰撞次数增加（适用于：全子弹穿透）
        public int PointExtraAttackTimes;                // 点数修正额外攻击次数

        // 爆炸相关
        public float BombRangeRateAdd;                   // 爆炸范围增加（适用于：子弹爆炸、导弹爆炸等）
        public float BombDamageInGameAmplifyAdd;         // 爆炸伤害 局内点数增加（适用于：子弹爆炸、导弹爆炸等）

        // 分裂系统
        public int BulletSplitCount;                     // 子弹可分裂 分裂数量
        public float SplitDamageInGameAmplifyAdd;        // 分裂强化伤害 局内点数增加
        public float ScatterDamageInGameAmplifyAdd;      // 散射强化伤害 局内点数增加
        public float SkillCdReducePercent;               // 技能CD减少百分比
        public float SkillCdTimeSecondAdd;               // 技能CD增量S

        public void Add(in UnitSkillFeature other)
        {
            BulletCountAdd += other.BulletCountAdd;
            CollisionCountAdd += other.CollisionCountAdd;
            SubBulletCollisionCountAdd += other.SubBulletCollisionCountAdd;
            PointExtraAttackTimes += other.PointExtraAttackTimes;

            BombRangeRateAdd += other.BombRangeRateAdd;
            BombDamageInGameAmplifyAdd += other.BombDamageInGameAmplifyAdd;

            BulletSplitCount += other.BulletSplitCount;
            SplitDamageInGameAmplifyAdd += other.SplitDamageInGameAmplifyAdd;
            ScatterDamageInGameAmplifyAdd += other.ScatterDamageInGameAmplifyAdd;
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
