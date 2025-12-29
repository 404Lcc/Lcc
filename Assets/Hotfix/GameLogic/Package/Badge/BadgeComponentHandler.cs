namespace LccHotfix
{
    public interface IBadgeComponent
    {
        BadgeConfig GetConfig(string badgeName);
        void RefreshDisplay();
    }

    public class BadgeComponentHandler : BadgeHandler
    {
        protected IBadgeComponent _component;

        public void InitComponent(IBadgeComponent component) => _component = component;

        protected override BadgeConfig GetConfig(string badgeName)
        {
            return _component.GetConfig(badgeName);
        }

        protected override void RefreshDisplay()
        {
            _component.RefreshDisplay();
        }
    }
}