namespace LccHotfix
{
    public static class WNodeExtension
    {
        public static Window OpenChild(this WNode openBy, string windowName, object[] param = null)
        {
            return Main.WindowService.OpenWindow(openBy, windowName, param);
        }
    }
}