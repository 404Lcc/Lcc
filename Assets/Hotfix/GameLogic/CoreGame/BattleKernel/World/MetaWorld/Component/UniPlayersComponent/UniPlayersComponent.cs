using System.Collections.Generic;

namespace LccHotfix
{


    public class UniPlayersComponent : MetaComponent
    {
        private List<InGamePlayerInfo> mPlayerInfoList = new();
        public List<InGamePlayerInfo> PlayerInfoList => mPlayerInfoList;


        private InGamePlayerInfo mLocalPlayerInfoRef = null;
        public InGamePlayerInfo LocalPlayerInfoRef => mLocalPlayerInfoRef;

        public int PlayerCount => mPlayerInfoList.Count;

        public override void DisposeOnRemove()
        {
            mPlayerInfoList?.Clear();
            mLocalPlayerInfoRef = null;
        }

        public InGamePlayerInfo GetPlayerInfo(long playerUid)
        {
            if (mPlayerInfoList == null)
            {
                if (BattleLogger.IsDebugEnabled)
                    BattleLogger.LogError($"GetPlayerInfo mPlayerInfoList == null, playerUid={playerUid}");
                return null;
            }
            foreach (var info in mPlayerInfoList)
            {
                if (info.PlayerUid == playerUid)
                {
                    return info;
                }
            }
            if (BattleLogger.IsDebugEnabled)
                BattleLogger.LogError($"GetPlayerInfo return null; playerUid={playerUid}, PlayerCount={PlayerCount}");
            return null;
        }

        public InGamePlayerInfo GetPlayerInfoByIndex(int index)
        {
            if (mPlayerInfoList == null)
            {
                if (BattleLogger.IsDebugEnabled)
                    BattleLogger.LogError($"GetPlayerInfo mPlayerInfoList == null, index={index}");
                return null;
            }
            if (index >= 0 && index < PlayerCount)
            {
                return mPlayerInfoList[index];
            }
            if (BattleLogger.IsDebugEnabled)
                BattleLogger.LogError($"GetPlayerInfo return null; index={index}, PlayerCount={PlayerCount}");
            return null;
        }
        
        public void InitPlayerInfoList(List<InGamePlayerInfo> playerInfoList)
        {
            if (playerInfoList == null)
            {
                return;
            }

            mPlayerInfoList = playerInfoList;
            mLocalPlayerInfoRef = null;

            for (int i = 0; i < mPlayerInfoList.Count; i++)
            {
                var info = mPlayerInfoList[i];
                if (info.IsLocalPlayer)
                {
                    if (mLocalPlayerInfoRef != null)
                    {
                        BattleLogger.LogError($"InitPlayerInfoList exist_uid={mLocalPlayerInfoRef.PlayerUid}, new_uid={info.PlayerUid}");
                    }

                    mLocalPlayerInfoRef = info;
                }
            }
        }
    }

    public partial class MetaWorld
    {
        public UniPlayersComponent comUniPlayers
        {
            get { return GetUniqueComponent<UniPlayersComponent>(MetaComponentsLookup.ComUniPlayers); }
        }

        public bool hasComUniPlayers
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniPlayers); }
        }

        public void SetComUniPlayers(List<InGamePlayerInfo> playerInfoList)
        {
            var index = MetaComponentsLookup.ComUniPlayers;
            var component = (UniPlayersComponent)UniqueEntity.CreateComponent(index, typeof(UniPlayersComponent));
            component.InitPlayerInfoList(playerInfoList);
            SetUniqueComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex _ComUniPlayersIndex = new(typeof(UniPlayersComponent));
        public static int ComUniPlayers => _ComUniPlayersIndex.Index;
    }
}
