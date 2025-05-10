using System.Collections.Generic;

namespace LccHotfix
{
    public class InGamePlayerInfo
    {
        public long PlayerUID { get; set; }
        public int PlayerIndex { get; set; }
        public bool IsLocalPlayer { get; set; }
        public KVContext PlayerContext { get; set; } = new KVContext();
        public List<CharacterData> CharacterDataList { get; set; } = new List<CharacterData>();
        public PlayerSimpleData PlayerSimpleData { get; set; }
    }


    public class ComInGamePlayers : MetaComponent
    {
        public List<InGamePlayerInfo> PlayerInfoList { get; private set; } = new List<InGamePlayerInfo>();
        public InGamePlayerInfo LocalPlayerInfo { get; private set; }

        public void InitPlayerInfoList(List<InGamePlayerInfo> playerInfoList)
        {
            if (playerInfoList == null || playerInfoList.Count == 0)
            {
                return;
            }

            PlayerInfoList.AddRange(playerInfoList);
            LocalPlayerInfo = null;

            for (int i = 0; i < PlayerInfoList.Count; i++)
            {
                var info = PlayerInfoList[i];
                if (info.IsLocalPlayer)
                {
                    LocalPlayerInfo = info;
                }
            }
        }

        public override void Dispose()
        {
            PlayerInfoList.Clear();
            LocalPlayerInfo = null;
        }

        public InGamePlayerInfo GetPlayerInfo(long playerUid)
        {
            foreach (var info in PlayerInfoList)
            {
                if (info.PlayerUID == playerUid)
                {
                    return info;
                }
            }

            return null;
        }
    }

    public partial class MetaContext
    {
        public MetaEntity comInGamePlayersEntity
        {
            get { return GetGroup(MetaMatcher.ComInGamePlayers).GetSingleEntity(); }
        }

        public ComInGamePlayers comInGamePlayers
        {
            get { return comInGamePlayersEntity.comUniPlayers; }
        }

        public bool hasComUniPlayers
        {
            get { return comInGamePlayersEntity != null; }
        }

        public MetaEntity SetComInGamePlayers(List<InGamePlayerInfo> playerInfoList)
        {
            if (hasComUniPlayers)
            {
                var entity = comInGamePlayersEntity;
                entity.ReplaceComInGamePlayers(playerInfoList);
                return entity;
            }
            else
            {
                var entity = CreateEntity();
                entity.AddComInGamePlayers(playerInfoList);
                return entity;
            }
        }

        public InGamePlayerInfo GetPlayerInfo(long playerUID)
        {
            if (hasComUniPlayers)
            {
                var entity = comInGamePlayersEntity;
                return entity.comUniPlayers.GetPlayerInfo(playerUID);
            }

            return null;
        }

        public KVContext GetPlayerContext(long playerUID)
        {
            var playerInfo = GetPlayerInfo(playerUID);
            if (playerInfo == null)
            {
                return null;
            }

            return playerInfo.PlayerContext;
        }
    }

    public partial class MetaEntity
    {
        public ComInGamePlayers comUniPlayers
        {
            get { return (ComInGamePlayers)GetComponent(MetaComponentsLookup.ComInGamePlayers); }
        }

        public bool hasComUniPlayers
        {
            get { return HasComponent(MetaComponentsLookup.ComInGamePlayers); }
        }

        public void AddComInGamePlayers(List<InGamePlayerInfo> playerInfoList)
        {
            var index = MetaComponentsLookup.ComInGamePlayers;
            var component = (ComInGamePlayers)CreateComponent(index, typeof(ComInGamePlayers));
            component.InitPlayerInfoList(playerInfoList);
            AddComponent(index, component);
        }

        public void ReplaceComInGamePlayers(List<InGamePlayerInfo> playerInfoList)
        {
            var index = MetaComponentsLookup.ComInGamePlayers;
            var component = (ComInGamePlayers)CreateComponent(index, typeof(ComInGamePlayers));
            component.InitPlayerInfoList(playerInfoList);
            ReplaceComponent(index, component);
        }

        public void RemoveComInGamePlayers()
        {
            var index = MetaComponentsLookup.ComInGamePlayers;
            var component = (ComInGamePlayers)CreateComponent(index, typeof(ComInGamePlayers));
            RemoveComponent(MetaComponentsLookup.ComInGamePlayers);
        }
    }

    public sealed partial class MetaMatcher
    {
        static Entitas.IMatcher<MetaEntity> _matcherComInGamePlayers;

        public static Entitas.IMatcher<MetaEntity> ComInGamePlayers
        {
            get
            {
                if (_matcherComInGamePlayers == null)
                {
                    var matcher = (Entitas.Matcher<MetaEntity>)Entitas.Matcher<MetaEntity>.AllOf(MetaComponentsLookup.ComInGamePlayers);
                    matcher.ComponentNames = MetaComponentsLookup.componentNames;
                    _matcherComInGamePlayers = matcher;
                }

                return _matcherComInGamePlayers;
            }
        }
    }

    public static partial class MetaComponentsLookup
    {
        public static int ComInGamePlayers;
    }
}