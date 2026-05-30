namespace LccHotfix
{
    public class LifeComponent : LogicComponent
    {
        public float duration;

        public void Init(float duration)
        {
            this.duration = duration;
        }
    }

    public partial class LogicEntity
    {
        public LifeComponent comLife
        {
            get { return (LifeComponent)GetComponent(LogicComponentsLookup.ComLife); }
        }

        public bool hasComLife
        {
            get { return HasComponent(LogicComponentsLookup.ComLife); }
        }

        public void AddComLife(float duration)
        {
            if (hasComLife)
            {
                return;
            }
            var index = LogicComponentsLookup.ComLife;
            var component = (LifeComponent)CreateComponent(index, typeof(LifeComponent));
            component.Init(duration);
            AddComponent(index, component);
        }

        public void ReplaceComLife(float duration)
        {
            var index = LogicComponentsLookup.ComLife;
            var component = (LifeComponent)CreateComponent(index, typeof(LifeComponent));
            component.Init(duration);
            ReplaceComponent(index, component);
        }

        public void RemoveComLife()
        {
            if (hasComLife)
            {
                RemoveComponent(LogicComponentsLookup.ComLife);
            }
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComLifeIndex = new(typeof(LifeComponent));
        public static int ComLife => _ComLifeIndex.Index;
    }
}
