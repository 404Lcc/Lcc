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

    public sealed partial class LogicMatcher
    {
        private static Entitas.IMatcher<LogicEntity> _matcherComControl;

        public static Entitas.IMatcher<LogicEntity> ComControl
        {
            get
            {
                if (_matcherComControl == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComControl);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComControl = matcher;
                }

                return _matcherComControl;
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        public static int ComControl;
    }
}