using System;

namespace LccHotfix
{
    public class ComOwnerPlayer : LogicComponent
    {
        public long UID { get; private set; }
        public InGamePlayerData PlayerData { get; private set; }

        public void Init(InGamePlayerData data)
        {
            PlayerData = data;
            UID = data.PlayerUID;
        }
    }

    public partial class LogicEntity
    {

        public ComOwnerPlayer comOwnerPlayer
        {
            get { return (ComOwnerPlayer)GetComponent(LogicComponentsLookup.ComOwnerPlayer); }
        }

        public bool hasComOwnerPlayer
        {
            get { return HasComponent(LogicComponentsLookup.ComOwnerPlayer); }
        }

        public void AddComOwnerPlayer(InGamePlayerData data)
        {
            var index = LogicComponentsLookup.ComOwnerPlayer;
            var component = (ComOwnerPlayer)CreateComponent(index, typeof(ComOwnerPlayer));
            component.Init(data);
            AddComponent(index, component);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComOwnerPlayerIndex = new ComponentTypeIndex(typeof(ComOwnerPlayer));
        public static int ComOwnerPlayer => ComOwnerPlayerIndex.index;
    }
}