namespace LccHotfix
{
    public static class PanelTypeExtension
    {
        public static string ToPanelString(this PanelType type)
        {
            return $"{type}Panel";
        }
    }
}