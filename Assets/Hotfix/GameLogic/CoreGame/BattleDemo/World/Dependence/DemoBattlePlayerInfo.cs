namespace LccHotfix
{
    public class DemoBattlePlayerInfo : IBattlePlayerInfo
    {
        public DemoBattlePlayerInfo(long playerUid, int playerIndex, bool isLocalPlayer)
        {
            PlayerUid = playerUid;
            PlayerIndex = playerIndex;
            IsLocalPlayer = isLocalPlayer;
        }

        public long PlayerUid { get; }

        public int PlayerIndex { get; }

        public bool IsLocalPlayer { get; }
    }
}
