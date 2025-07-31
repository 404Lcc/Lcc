using System;

namespace LccHotfix
{
    //死亡过程
    public interface IDeathProcess
    {
        public LogicEntity Entity { get; set; }

        void Start();
        void Update();
        bool IsFinished();
    }

    public class ComDeath : LogicComponent
    {
        public IDeathProcess deathProcess;
    }

    public partial class LogicEntity
    {
        public ComDeath comDeath { get { return (ComDeath)GetComponent(LogicComponentsLookup.ComDeath); } }
        public bool hasComDeath { get { return HasComponent(LogicComponentsLookup.ComDeath); } }

        public void Death(IDeathProcess deathProcess = null)
        {
            if (hasComDeath)
            {
                return;
            }

            var index = LogicComponentsLookup.ComDeath;
            var component = (ComDeath)CreateComponent(index, typeof(ComDeath));
            component.deathProcess = deathProcess;

            if (deathProcess != null)
            {
                deathProcess.Entity = this;
                deathProcess.Start();
            }
            AddComponent(index, component);
        }
    }

    public sealed partial class LogicMatcher
    {
        private static Entitas.IMatcher<LogicEntity> _matcherComDeath;

        public static Entitas.IMatcher<LogicEntity> ComDeath
        {
            get
            {
                if (_matcherComDeath == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComDeath);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComDeath = matcher;
                }

                return _matcherComDeath;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComDeath;
    }
}