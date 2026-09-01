namespace LccHotfix
{
    /// <summary>
    /// LogicEntity 归属玩家查询相关扩展。
    /// </summary>
    public static class LogicEntityOwnerExtensions
    {
        public static InGamePlayerInfo GetPlayerInfo(this LogicEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            if (!entity.hasComOwnerPlayer)
            {
                if (BattleLogger.IsDebugEnabled && entity.hasComHero)
                    BattleLogger.LogError($"GetPlayerInfo !entity.hasComOwnerPlayer");

                if (entity.hasComHolder)
                {
                    var ownerEntity = entity.OwnerWorld.GetEntityWithComID(entity.comHolder.HolderEntityID);
                    return GetPlayerInfo(ownerEntity);
                }

                return null;
            }

            var comOwnerPlayer = entity.comOwnerPlayer;
            return comOwnerPlayer.PlayerInfoRef as InGamePlayerInfo;
        }
    }
}
