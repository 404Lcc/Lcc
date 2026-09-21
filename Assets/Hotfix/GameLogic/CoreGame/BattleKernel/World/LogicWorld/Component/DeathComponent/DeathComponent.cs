namespace LccHotfix
{
    public class DeathComponent : LogicComponent
    {
        private CustomLogic _deathProcess;

        public CustomLogic DeathProcess
        {
            get { return _deathProcess; }
            set { _deathProcess = value; }
        }

        /// <summary>是否正常击杀。false 表示机制销毁（离开地图等）。</summary>
        public bool IsNormalKilled = true;

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();
            IsNormalKilled = true;
            _deathProcess = null;
        }
    }

    public partial class LogicEntity
    {
        public DeathComponent comDeath
        {
            get { return (DeathComponent)GetComponent(LogicComponentsLookup.ComDeath); }
        }

        public bool hasComDeath
        {
            get { return HasComponent(LogicComponentsLookup.ComDeath); }
        }

        public void AddComDeath(CustomLogic deathProcess)
        {
            if (hasComDeath)
            {
                BattleLogger.LogError($"AddComDeath already hasComDeath!");
                return;
            }

            var index = LogicComponentsLookup.ComDeath;
            var component = (DeathComponent)CreateComponent(index, typeof(DeathComponent));
            component.DeathProcess = deathProcess;
            AddComponent(index, component);
        }

        public void ReplaceComDeath(CustomLogic newDeathProcess)
        {
            var index = LogicComponentsLookup.ComDeath;
            var component = (DeathComponent)CreateComponent(index, typeof(DeathComponent));
            component.DeathProcess = newDeathProcess;
            ReplaceComponent(index, component);
        }

        public void RemoveComDeath()
        {
            RemoveComponent(LogicComponentsLookup.ComDeath);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComDeathIndex = new(typeof(DeathComponent));
        public static int ComDeath => _ComDeathIndex.Index;
    }
}