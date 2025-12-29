namespace LccHotfix
{
    public class ComControl : LogicComponent
    {
        private IControl control;

        public T GetControl<T>() where T : class, IControl
        {
            return (T)control;
        }

        public void SetControl(IControl control)
        {
            this.control = control;
        }

        public override void Dispose()
        {
            base.Dispose();
            control.Dispose();
            control = null;
        }
    }

    public partial class LogicEntity
    {
        public ComControl comControl
        {
            get { return (ComControl)GetComponent(LogicComponentsLookup.ComControl); }
        }

        public bool hasComControl
        {
            get { return HasComponent(LogicComponentsLookup.ComControl); }
        }

        public void AddComControl<T>() where T : IControl, new()
        {
            var index = LogicComponentsLookup.ComControl;
            var component = (ComControl)CreateComponent(index, typeof(ComControl));
            AddComponent(index, component);
            var control = new T();
            control.Entity = this;
            component.SetControl(control);
        }

        public T GetControl<T>() where T : class, IControl
        {
            if (!hasComControl)
                return null;
            return comControl.GetControl<T>();
        }

        public void RemoveComControl()
        {
            RemoveComponent(LogicComponentsLookup.ComControl);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComControlIndex = new ComponentTypeIndex(typeof(ComControl));
        public static int ComControl => ComControlIndex.index;
    }
}