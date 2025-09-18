using System.Collections.Generic;

namespace LccHotfix
{
    public class InGamePlayerData
    {
        public long PlayerUID { get; set; }
        public bool IsLocalPlayer { get; set; }
        public PlayerSimpleData PlayerSimpleData { get; set; }

        public void InitData(PlayerSimpleData data)
        {
            var mod = GameUtility.GetModel<ModPlayer>();
            PlayerUID = data.UID;
            IsLocalPlayer = mod.PlayerData.UID == PlayerUID;
            PlayerSimpleData = data;
        }
    }

    public class ComUniInGamePlayers : MetaComponent
    {
        public List<InGamePlayerData> PlayerList { get; private set; } = new List<InGamePlayerData>();
        public InGamePlayerData LocalPlayer { get; private set; }

        public void InitPlayerList(List<InGamePlayerData> list)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }

            PlayerList.Clear();
            PlayerList.AddRange(list);

            LocalPlayer = null;

            for (int i = 0; i < PlayerList.Count; i++)
            {
                var data = PlayerList[i];
                if (data.IsLocalPlayer)
                {
                    LocalPlayer = data;
                }
            }
        }

        public InGamePlayerData GetPlayer(long playerUID)
        {
            foreach (var item in PlayerList)
            {
                if (item.PlayerUID == playerUID)
                {
                    return item;
                }
            }

            return null;
        }

        public void AddPlayer(InGamePlayerData data)
        {
            foreach (var item in PlayerList)
            {
                if (item.PlayerUID == data.PlayerUID)
                {
                    return;
                }
            }

            List<InGamePlayerData> list = new List<InGamePlayerData>();
            list.Add(data);

            Owner.ReplaceComUniInGamePlayers(list);
        }

        public void RemovePlayer(InGamePlayerData data)
        {
            List<InGamePlayerData> list = new List<InGamePlayerData>();
            list.AddRange(PlayerList);

            foreach (var item in PlayerList)
            {
                if (item.PlayerUID == data.PlayerUID)
                {
                    list.Remove(item);
                }
            }

            Owner.ReplaceComUniInGamePlayers(list);
        }

        public override void Dispose()
        {
            base.Dispose();

            PlayerList.Clear();
            LocalPlayer = null;
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

        public MetaEntity SetComUniInGamePlayers(List<InGamePlayerData> list)
        {
            if (hasComUniInGamePlayers)
            {
                var entity = comUniInGamePlayersEntity;
                entity.ReplaceComUniInGamePlayers(list);
                return entity;
            }
            else
            {
                var entity = CreateEntity();
                entity.AddComUniInGamePlayers(list);
                return entity;
            }
        }

        public InGamePlayerData GetPlayer(long playerUID)
        {
            if (hasComUniInGamePlayers)
            {
                var entity = comUniInGamePlayersEntity;
                return entity.comUniInGamePlayers.GetPlayer(playerUID);
            }

            return null;
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

        public void AddComUniInGamePlayers(List<InGamePlayerData> list)
        {
            var index = MetaComponentsLookup.ComUniInGamePlayers;
            var component = (ComUniInGamePlayers)CreateComponent(index, typeof(ComUniInGamePlayers));
            component.InitPlayerList(list);
            AddComponent(index, component);
        }

        public void ReplaceComUniInGamePlayers(List<InGamePlayerData> list)
        {
            var index = MetaComponentsLookup.ComUniInGamePlayers;
            var component = (ComUniInGamePlayers)CreateComponent(index, typeof(ComUniInGamePlayers));
            component.InitPlayerList(list);
            ReplaceComponent(index, component);
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