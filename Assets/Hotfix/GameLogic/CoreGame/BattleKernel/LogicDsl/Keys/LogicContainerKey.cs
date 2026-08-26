namespace LccHotfix
{
    /// <summary>
    /// CustomLogic 配置容器名称集合，用于按逻辑类型定位对应配置表。
    /// </summary>
    public partial class LogicContainerKey
    {
        /// <summary>
        /// 玩法模式逻辑配置容器。
        /// </summary>
        public static string LogicConfigs_GameMode = "CCN_GameMode";

        /// <summary>
        /// 战斗实体主状态机逻辑配置容器。
        /// </summary>
        public static string LogicConfigs_EntityFSM = "CCN_EntityFSM";

        /// <summary>
        /// 技能逻辑配置容器。
        /// </summary>
        public static string LogicConfigs_Skill = "CCN_EntitySkill";

        /// <summary>
        /// 子物体逻辑配置容器。
        /// </summary>
        public static string LogicConfigs_Subobject = "CCN_EntitySubobject";

        /// <summary>
        /// 关卡逻辑配置容器。
        /// </summary>
        public static string LogicConfigs_Level = "CCN_Level";

        /// <summary>
        /// Buff 逻辑配置容器。
        /// </summary>
        public static string LogicConfigs_Buff = "CCN_Buff";

        /// <summary>
        /// AI 逻辑配置容器。
        /// </summary>
        public static string LogicConfigs_AI = "CCN_AI";

        /// <summary>
        /// 英雄被动技能逻辑配置容器。
        /// </summary>
        public static string LogicConfigs_PassiveSkill = "CCN_PassiveSkill";
    }
}
