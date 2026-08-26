namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 在 FSM 黑板上的运行时变量读写扩展。
    /// </summary>
    public static class LogicEntityFsmVarExtensions
    {
        /// <summary>
        /// 从实体 FSM 黑板读取变量；实体无效或无 FSM 时返回默认值。
        /// </summary>
        public static T GetFsmVar<T>(this LogicEntity entity, string key, T defaultValue = default)
        {
            if (entity == null || !entity.hasComFSM)
                return defaultValue;

            return entity.comFSM.Logic.GetVar(key, defaultValue);
        }

        /// <summary>
        /// 向实体 FSM 黑板写入变量；实体无效或无 FSM 时忽略。
        /// </summary>
        public static void SetFsmVar<T>(this LogicEntity entity, string key, T value)
        {
            if (entity == null || !entity.hasComFSM)
                return;

            entity.comFSM.Logic.SetVar(key, value);
        }
    }
}