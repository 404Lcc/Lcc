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
        public ComDeath comDeath
        {
            get { return (ComDeath)GetComponent(LogicComponentsLookup.ComDeath); }
        }

        public bool hasComDeath
        {
            get { return HasComponent(LogicComponentsLookup.ComDeath); }
        }

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

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComDeathIndex = new ComponentTypeIndex(typeof(ComDeath));
        public static int ComDeath => ComDeathIndex.index;
    }
}