namespace LccHotfix
{
    /// <summary>
    /// MetaWorld 玩家信息查询扩展。
    /// </summary>
    public static class MetaWorldPlayerExtensions
    {
        public static InGamePlayerInfo GetPlayerInfo(this MetaWorld world, long playerUid)
        {
            if (!world.hasComUniPlayers)
            {
                if (BattleLogger.IsDebugEnabled)
                    BattleLogger.LogError($"GetPlayerInfo world.meta.hasComUniPlayers = false");
                return null;
            }

            var comInGamePlayers = world.comUniPlayers;
            if (comInGamePlayers == null)
            {
                if (BattleLogger.IsDebugEnabled)
                    BattleLogger.LogError($"GetPlayerInfo comInGamePlayers == null");
                return null;
            }

            return comInGamePlayers.GetPlayerInfo(playerUid);
        }

        public static InGamePlayerInfo GetLocalPlayerInfo(this MetaWorld world)
        {
            if (!world.hasComUniPlayers)
            {
                BattleLogger.LogError($"GetLocalPlayerInfo world.meta.hasComUniPlayers = false");
                return null;
            }

            var comInGamePlayers = world.comUniPlayers;
            if (comInGamePlayers == null)
            {
                BattleLogger.LogError($"GetLocalPlayerInfo comInGamePlayers == null");
                return null;
            }

            return comInGamePlayers.PlayerInfoList[0];
        }

        public static IPlayerInfo GetMonsterPlayerInfo(this MetaWorld world)
        {
            if (!world.hasComUniPlayers)
            {
                BattleLogger.LogError($"GetMonsterPlayerInfo world.meta.hasComUniPlayers = false");
                return null;
            }

            var comInGamePlayers = world.comUniPlayers;
            if (comInGamePlayers == null)
            {
                BattleLogger.LogError($"GetMonsterPlayerInfo comInGamePlayers == null");
                return null;
            }

            return comInGamePlayers.PlayerInfoList[1];
        }
    }
}
