using System;
using PBConfig;

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
        public int SourceLogicConfigID;
        public uint BattleSupplyId; // 出手补给卡tid，没有则为0
        public uint BattleSupplySetId; // 归属的补给集合id，补给标优先，否则按英雄普攻技能归集
        
        public double SkillDamageFactor;
        public double SkillFixedDamage;
        public double StageDamageFactor;
        
        public double FinalFixedDamage;

        public EDamageType DamageType;
        public TElementType ElementType;


        public long Timestamp;
        public Random Random;
        public double ExtraCritDamage;
        public float RandomFinalDamageRate; //随机波动
        public bool ForceCritical; // 强制暴击（跳过暴击概率）
    }
}
