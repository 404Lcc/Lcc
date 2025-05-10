using System;

namespace LccHotfix
{
    public class ComOwnerPlayer : LogicComponent
    {
        public long UID { get; private set; }
        public InGamePlayerInfo PlayerInfo { get; private set; }

        public void Init(InGamePlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
            UID = playerInfo.PlayerUID;
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

        public void AddComOwnerPlayer(InGamePlayerInfo playerInfo)
        {
            var index = LogicComponentsLookup.ComOwnerPlayer;
            var component = (ComOwnerPlayer)CreateComponent(index, typeof(ComOwnerPlayer));
            component.Init(playerInfo);
            AddComponent(index, component);
        }
    }

    public sealed partial class LogicMatcher
    {

        static Entitas.IMatcher<LogicEntity> _matcherComOwnerPlayer;

        public static Entitas.IMatcher<LogicEntity> ComOwnerPlayer
        {
            get
            {
                if (_matcherComOwnerPlayer == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComOwnerPlayer);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComOwnerPlayer = matcher;
                }

                return _matcherComOwnerPlayer;
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        public static int ComOwnerPlayer;
    }
}