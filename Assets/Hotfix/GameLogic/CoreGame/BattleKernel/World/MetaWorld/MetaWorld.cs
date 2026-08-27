namespace LccHotfix
{
    public partial class MetaWorld
    {
        public InGamePlayerInfo GetPlayerInfo(long playerUid)
        {
            if (!hasComUniPlayers)
            {
                if (BattleLogger.IsDebugEnabled)
                    BattleLogger.LogError($"GetPlayerInfo world.meta.hasComUniPlayers = false");
                return null;
            }

            var comInGamePlayers = comUniPlayers;
            if (comInGamePlayers == null)
            {
                if (BattleLogger.IsDebugEnabled)
                    BattleLogger.LogError($"GetPlayerInfo comInGamePlayers == null");
                return null;
            }

            return comInGamePlayers.GetPlayerInfo(playerUid);
        }

        public InGamePlayerInfo GetLocalPlayerInfo()
        {
            if (!hasComUniPlayers)
            {
                BattleLogger.LogError($"GetLocalPlayerInfo world.meta.hasComUniPlayers = false");
                return null;
            }

            var comInGamePlayers = comUniPlayers;
            if (comInGamePlayers == null)
            {
                BattleLogger.LogError($"GetLocalPlayerInfo comInGamePlayers == null");
                return null;
            }

            return comInGamePlayers.PlayerInfoList[0];
        }

        public IPlayerInfo GetMonsterPlayerInfo()
        {
            if (!hasComUniPlayers)
            {
                BattleLogger.LogError($"GetMonsterPlayerInfo world.meta.hasComUniPlayers = false");
                return null;
            }

            var comInGamePlayers = comUniPlayers;
            if (comInGamePlayers == null)
            {
                BattleLogger.LogError($"GetMonsterPlayerInfo comInGamePlayers == null");
                return null;
            }

            return comInGamePlayers.PlayerInfoList[1];
        }

    }
}