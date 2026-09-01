namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 战斗单位标签相关扩展。
    /// </summary>
    public static class LogicEntityBattleUnitTagExtensions
    {
        /// <summary>
        /// 尝试获取战斗单位标签信息。
        /// </summary>
        public static bool GetBattleUnitTag(this LogicEntity entity, out BattleUnitTag unitTag)
        {
            if (entity.hasComBattleUnitTag)
            {
                unitTag = entity.comBattleUnitTag.Tag;
                return true;
            }

            BattleLogger.LogError("GetBattleUnitTag !entity.hasComBattleUnitTag");
            unitTag = default;
            return false;
        }
    }
}
