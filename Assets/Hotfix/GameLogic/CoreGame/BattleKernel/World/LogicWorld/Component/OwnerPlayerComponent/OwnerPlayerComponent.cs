namespace LccHotfix
{
    public interface IBattlePlayerInfo
    {
        long PlayerUid { get; }
        int PlayerIndex { get; }
        bool IsLocalPlayer { get; }
    }
    
    public class OwnerPlayerComponent : LogicComponent
    {
        public long UID { get; private set; }
        public IBattlePlayerInfo PlayerInfoRef { get; private set; }

        public void Init(IBattlePlayerInfo playerInfoRef)
        {
            PlayerInfoRef = playerInfoRef;
            UID = playerInfoRef.PlayerUid;
        }
    }

    public partial class LogicEntity
    {
        public OwnerPlayerComponent comOwnerPlayer
        {
            get { return (OwnerPlayerComponent)GetComponent(LogicComponentsLookup.ComOwnerPlayer); }
        }

        public bool hasComOwnerPlayer
        {
            get { return HasComponent(LogicComponentsLookup.ComOwnerPlayer); }
        }

        
        public void AddComOwnerPlayer(IBattlePlayerInfo playerInfo)
        {
            var index = LogicComponentsLookup.ComOwnerPlayer;
            var component = (OwnerPlayerComponent)CreateComponent(index, typeof(OwnerPlayerComponent));
            component.Init(playerInfo);
            AddComponent(index, component);
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComOwnerPlayerIndex = new(typeof(OwnerPlayerComponent));
        public static int ComOwnerPlayer => _ComOwnerPlayerIndex.Index;
    }
}
