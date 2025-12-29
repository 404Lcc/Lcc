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

        public void UpdatePlayer(InGamePlayerData data)
        {
            InGamePlayerData updateData = null;
            foreach (var item in PlayerList)
            {
                if (item.PlayerUID == data.PlayerUID)
                {
                    updateData = item;
                    break;
                }
            }

            if (updateData != null)
            {
                PlayerList.Remove(updateData);
                PlayerList.Add(data);

                Owner.ReplaceComponent(MetaComponentsLookup.ComUniInGamePlayers, this);
            }
            else
            {
                AddPlayer(data);
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

            PlayerList.Add(data);

            Owner.ReplaceComponent(MetaComponentsLookup.ComUniInGamePlayers, this);
        }

        public void RemovePlayer(InGamePlayerData data)
        {
            for (int i = PlayerList.Count - 1; i >= 0; i--)
            {
                var item = PlayerList[i];
                if (item.PlayerUID == data.PlayerUID)
                {
                    PlayerList.Remove(item);
                }
            }

            Owner.ReplaceComponent(MetaComponentsLookup.ComUniInGamePlayers, this);
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
        public ComUniInGamePlayers ComUniInGamePlayers
        {
            get { return GetUniqueComponent<ComUniInGamePlayers>(MetaComponentsLookup.ComUniInGamePlayers); }
        }

        public bool hasComUniInGamePlayers
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniInGamePlayers); }
        }

        public void SetComUniInGamePlayers(List<InGamePlayerData> list)
        {
            var index = MetaComponentsLookup.ComUniInGamePlayers;
            var component = (ComUniInGamePlayers)UniqueEntity.CreateComponent(index, typeof(ComUniInGamePlayers));
            component.InitPlayerList(list);
            SetUniqueComponent(index, component);
        }

        public InGamePlayerData GetPlayer(long playerUID)
        {
            if (hasComUniInGamePlayers)
            {
                return ComUniInGamePlayers.GetPlayer(playerUID);
            }

            return null;
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex ComUniInGamePlayersIndex = new ComponentTypeIndex(typeof(ComUniInGamePlayers));
        public static int ComUniInGamePlayers => ComUniInGamePlayersIndex.index;
    }
}