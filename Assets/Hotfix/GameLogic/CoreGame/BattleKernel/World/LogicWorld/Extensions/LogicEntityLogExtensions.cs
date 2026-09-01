namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 日志标识相关扩展。
    /// </summary>
    public static class LogicEntityLogExtensions
    {
        /// <summary>
        /// 获取实体日志标识；以 ID 为基准，按组件存在情况逐项追加字段，便于扩展。
        /// </summary>
        public static string GetLogStr(this LogicEntity entity)
        {
            if (entity == null)
            {
                return "null";
            }

            var logStr = $"(ID:{entity.ID}";

            if (entity.hasComSubobject)
            {
                logStr += $",Subobj:{entity.comSubobject.ConfigId}";
            }

            if (entity.hasComBattleUnitTag)
            {
                logStr += $",BattleUnit:{entity.comBattleUnitTag.Tag.BattleUnitId}";
            }

            if (entity.hasComFSM && entity.comFSM?.Logic != null)
            {
                var logic = entity.comFSM.Logic;
                logStr += $",Fsm:{logic.GenInfo?.LogicConfigID ?? 0},State:{logic.MainFsmNode?.CurrentStateID ?? "null"}";
            }

            return logStr + ")";
        }
    }
}
