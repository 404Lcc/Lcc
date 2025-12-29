namespace LccHotfix
{
    public struct CalcDamageInfo
    {
        public DamageProp damageProp;
        public long defenderId;
        public float baseValue; //正数是伤害，负数是回复
        public float damageRate;
        public DamageSource source;
        public KVContext context;

        public float ResultValue => baseValue * damageRate;
    }
}