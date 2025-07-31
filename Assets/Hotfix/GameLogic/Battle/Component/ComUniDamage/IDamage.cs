namespace LccHotfix
{
    //TODO 抽到effect实现
    public interface IDamage
    {
        /// <summary>
        /// 伤害
        /// </summary>
        void ApplyDamageByRate(long attackerId, long defenderId, float rate, DamageSource source, KVContext context = null);
        void ApplyDamageByFixed(long attackerId, long defenderId, float fixedValue, DamageSource source, KVContext context = null);

        /// <summary>
        /// 恢复
        /// </summary>
        void ApplyRecoverByRate(long attackerId, long defenderId, float rate, DamageSource source, KVContext context = null);
        void ApplyRecoverByFixed(long attackerId, long defenderId, float fixedValue, DamageSource source, KVContext context = null);


        /// <summary>
        /// 应用
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        float ApplyChange(CalcDamageInfo info);

        /// <summary>
        /// 伤害相关
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        float ApplyDamageChange(CalcDamageInfo info);
        void PreDamageChange(CalcDamageInfo info);
        void PostDamageChange(CalcDamageInfo info);

        /// <summary>
        /// 恢复相关
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        float ApplyRecoverChange(CalcDamageInfo info);
        void PreRecoverChange(CalcDamageInfo info);
        void PostRecoverChange(CalcDamageInfo info);


        /// <summary>
        /// HP改变之后
        /// </summary>
        /// <param name="data"></param>
        void AfterChange(CalcDamageInfo info);




        /// <summary>
        /// 伤害率
        /// </summary>
        /// <param name="damageProp"></param>
        /// <returns></returns>
        float GetDamageRate(DamageProp prop);

        /// <summary>
        /// 计算暴击
        /// </summary>
        bool CalcCrit(DamageProp prop);

        /// <summary>
        /// 计算暴击伤害比例
        /// </summary>
        float CalcCritDamageRate(DamageProp prop);
    }
}