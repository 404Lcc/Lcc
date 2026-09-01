namespace LccHotfix
{
    /// <summary>
    /// HP 穿越 0 时的死亡边沿处理（与具体伤害入口解耦）。
    /// - &gt;0→≤0：打防重入标记、派发死亡事件、同步派发 Nt_Death（供 FSM/濒死 Buff 拦截）
    /// - 拦截后若回血：清除标记，允许再次死亡
    /// - ≤0→&gt;0（复活等）：清除标记
    /// </summary>
    public static class BattleDeathTransition
    {
        // StandaloneEntityCmdPreHandler 无状态，避免每次归零 new。
        private static readonly StandaloneEntityCmdPreHandler DeathCmdPreHandler = new();

        public static void Handle(LogicEntity owner, double oldHp, double newHp)
        {
            if (owner == null)
                return;

            if (oldHp > 0 && newHp <= 0)
            {
                if (owner.hasComBattleDeathTriggered)
                    return;

                owner.AddComBattleDeathTriggered();
                owner.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.DamagePolicyService?.DispatchTriggerDeath(owner);

                // 同步派发：濒死守护等需在当帧拦截并回血；不能走 CommandSender 队列拖到下一拍。
                var isIntercepted = DeathCmdPreHandler.PreHandleCommand(owner, new EntityCommand
                {
                    CmdType = EntityCmdType.Nt_Death,
                });
                if (isIntercepted && owner.hasComHp && owner.comHp.Hp > 0 && owner.hasComBattleDeathTriggered)
                    owner.RemoveComBattleDeathTriggered();
                return;
            }

            if (oldHp <= 0 && newHp > 0 && owner.hasComBattleDeathTriggered)
                owner.RemoveComBattleDeathTriggered();
        }
    }
}
