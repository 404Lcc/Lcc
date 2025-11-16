namespace LccHotfix
{
    public interface IBadgeComponent
    {
        void RefreshDisplay();
    }

    public class BadgeComponentHandler : BadgeHandler
    {
        protected IBadgeComponent _component;

        public void InitComponent(IBadgeComponent component) => _component = component;

        protected override void RefreshDisplay()
        {
            _component.RefreshDisplay();
        }
    }
}