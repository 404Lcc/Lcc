namespace LccHotfix
{
    /// <summary>
    /// 开火前/创建逻辑时/挂载后的玩法特化（弹药、英雄特性、星级倍率等）。
    /// Kernel 只负责通用技能流程装配。
    /// </summary>
    public interface ISkillFirePolicy
    {
        /// <summary>
        /// 开火前拦截与黑板写入。返回 false 表示不能开火。
        /// </summary>
        bool BeforeCreateSkillProcess(LogicEntity entity, int skillTid, VarEnv varEnv);

        /// <summary>
        /// 创建 SkillLogic 前改倍率/子物体等。
        /// </summary>
        void CustomizeSkillProcessLogic(LogicEntity entity, int skillTid, VarEnv varEnv);

        /// <summary>
        /// 技能已挂载后的扣弹与事件。
        /// </summary>
        void AfterCreateSkillProcess(LogicEntity entity, int skillTid, VarEnv varEnv);
    }
}
