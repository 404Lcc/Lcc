namespace LccModel
{
    public static class PanelTypeExpand
    {
        public static string ToPanelString(this PanelType type)
        {
            return type.ToString() + "Panel";
        }
    }
}