using System;

namespace LccHotfix
{
    public partial struct DamageContext
    {
        public LogicWorld World;
        public UnitSource Attacker;
        public UnitSource Defender;
        public SkillSource? Skill;
        public SubobjectSource? Subobject;
        public HitInfo? HitInfo;
        
        public double SkillDamageFactor;
        public double SkillFixedDamage;
        public double StageDamageFactor;
        
        public double FinalFixedDamage;

        public double FinalFixedReduceDamage; // 加的值，FinalFixedDamage有好多地方已经在用了，这里直接做一个负数的值吧。
        //public double FinalFixedDef;
        public EDamageType DamageType;

        //动态部分        
        // public int DamageType;
        // public double BaseDamage;
        public long Timestamp;
        // public double BaseAttack;
        public Random Random;
        public double ExtraCritDamage;
        public float RandomFinalDamageRate; //随机波动
    }
}
