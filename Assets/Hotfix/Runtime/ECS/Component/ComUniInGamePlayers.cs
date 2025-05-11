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


    public class ComUniInGamePlayers : MetaComponent
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
            base.Dispose();

            PlayerInfoList.Clear();
            LocalPlayerInfo = null;
        }

        public InGamePlayerInfo GetPlayerInfo(long playerUID)
        {
            foreach (var item in PlayerInfoList)
            {
                if (item.PlayerUID == playerUID)
                {
                    return item;
                }
            }

            return null;
        }
    }

    public partial class MetaContext
    {
        public MetaEntity comUniInGamePlayersEntity
        {
            get { return GetGroup(MetaMatcher.ComUniInGamePlayers).GetSingleEntity(); }
        }

        public ComUniInGamePlayers comUniInGamePlayers
        {
            get { return comUniInGamePlayersEntity.comUniInGamePlayers; }
        }

        public bool hasComUniInGamePlayers
        {
            get { return comUniInGamePlayersEntity != null; }
        }

        public MetaEntity SetComUniInGamePlayers(List<InGamePlayerInfo> playerInfoList)
        {
            if (hasComUniInGamePlayers)
            {
                var entity = comUniInGamePlayersEntity;
                entity.ReplaceComUniInGamePlayers(playerInfoList);
                return entity;
            }
            else
            {
                var entity = CreateEntity();
                entity.AddComUniInGamePlayers(playerInfoList);
                return entity;
            }
        }

        public InGamePlayerInfo GetPlayerInfo(long playerUID)
        {
            if (hasComUniInGamePlayers)
            {
                var entity = comUniInGamePlayersEntity;
                return entity.comUniInGamePlayers.GetPlayerInfo(playerUID);
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
        public ComUniInGamePlayers comUniInGamePlayers
        {
            get { return (ComUniInGamePlayers)GetComponent(MetaComponentsLookup.ComUniInGamePlayers); }
        }

        public bool hasComUniInGamePlayers
        {
            get { return HasComponent(MetaComponentsLookup.ComUniInGamePlayers); }
        }

        public void AddComUniInGamePlayers(List<InGamePlayerInfo> playerInfoList)
        {
            var index = MetaComponentsLookup.ComUniInGamePlayers;
            var component = (ComUniInGamePlayers)CreateComponent(index, typeof(ComUniInGamePlayers));
            component.InitPlayerInfoList(playerInfoList);
            AddComponent(index, component);
        }

        public void ReplaceComUniInGamePlayers(List<InGamePlayerInfo> playerInfoList)
        {
            var index = MetaComponentsLookup.ComUniInGamePlayers;
            var component = (ComUniInGamePlayers)CreateComponent(index, typeof(ComUniInGamePlayers));
            component.InitPlayerInfoList(playerInfoList);
            ReplaceComponent(index, component);
        }

        public void RemoveComUniInGamePlayers()
        {
            var index = MetaComponentsLookup.ComUniInGamePlayers;
            var component = (ComUniInGamePlayers)CreateComponent(index, typeof(ComUniInGamePlayers));
            RemoveComponent(MetaComponentsLookup.ComUniInGamePlayers);
        }
    }

    public sealed partial class MetaMatcher
    {
        static Entitas.IMatcher<MetaEntity> _matcherComUniInGamePlayers;

        public static Entitas.IMatcher<MetaEntity> ComUniInGamePlayers
        {
            get
            {
                if (_matcherComUniInGamePlayers == null)
                {
                    var matcher = (Entitas.Matcher<MetaEntity>)Entitas.Matcher<MetaEntity>.AllOf(MetaComponentsLookup.ComUniInGamePlayers);
                    matcher.ComponentNames = MetaComponentsLookup.componentNames;
                    _matcherComUniInGamePlayers = matcher;
                }

                return _matcherComUniInGamePlayers;
            }
        }
    }

    public static partial class MetaComponentsLookup
    {
        public static int ComUniInGamePlayers;
    }
}