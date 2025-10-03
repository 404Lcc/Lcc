namespace LccHotfix
{
    public class ComLife : LogicComponent
    {
        public float duration;

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    public partial class LogicEntity
    {
        public ComLife comLife
        {
            get { return (ComLife)GetComponent(LogicComponentsLookup.ComLife); }
        }

        public bool hasComLife
        {
            get { return HasComponent(LogicComponentsLookup.ComLife); }
        }

        public void AddComLife(float newDuration)
        {
            var index = LogicComponentsLookup.ComLife;
            var component = (ComLife)CreateComponent(index, typeof(ComLife));
            component.duration = newDuration;
            AddComponent(index, component);
        }

        public void ReplaceComLife(float newDuration)
        {
            var index = LogicComponentsLookup.ComLife;
            var component = (ComLife)CreateComponent(index, typeof(ComLife));
            component.duration = newDuration;
            ReplaceComponent(index, component);
        }

        public void RemoveComLife()
        {
            RemoveComponent(LogicComponentsLookup.ComLife);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComLifeIndex = new ComponentTypeIndex(typeof(ComLife));
        public static int ComLife => ComLifeIndex.index;
    }
}