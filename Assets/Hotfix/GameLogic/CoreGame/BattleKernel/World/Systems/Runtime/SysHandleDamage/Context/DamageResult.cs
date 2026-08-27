namespace LccHotfix
{
    public struct DamageResult
    {
        public double FinalDamage;
        public double ShieldDeducted; // 扣除的护盾值
        public bool IsCritical;
        public bool IsMiss;
        public bool IsBlock;
        public bool IsImmune;
        public bool IsInstantKill;
        public int HitCount;
        public DamageModifierInfo[] Modifiers;
        
        public static DamageResult Default => new DamageResult 
        { 
            FinalDamage = 0, 
            ShieldDeducted = 0,
            IsCritical = false, 
            IsMiss = false,
            IsBlock = false,
            IsImmune = false,
            IsInstantKill = false,
            HitCount = 1 
        };
    }
}
