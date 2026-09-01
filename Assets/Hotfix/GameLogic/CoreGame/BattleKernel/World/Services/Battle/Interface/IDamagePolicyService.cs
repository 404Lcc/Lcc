namespace LccHotfix
{
    public interface IDamagePolicyService
    {
        void ModifyDamageResult(in DamageContext context, LogicEntity defender, ref DamageResult result);
        void ModifyHeal(ref HealContext context);
        void DispatchTriggerDeath(LogicEntity entity);

        /// <summary>近战受击判定（供 Nt_OnHurt.V0）；无策略时视为非近战。</summary>
        bool IsMeleeHit(in DamageContext context);

        /// <summary>毒 DOT 等特殊飘字判定；无策略时视为普通伤害。</summary>
        bool IsPoisonDot(in DamageContext context);

        /// <summary>
        /// 扣血完成后的玩法收尾（击杀事件、周期打点等）。
        /// Kernel DamageHandler 只负责通用结算，产品逻辑经此回调下沉。
        /// </summary>
        void OnAfterDamageApplied(in DamageContext context, LogicEntity defender, in DamageResult result, int damageApplied);
    }
}
