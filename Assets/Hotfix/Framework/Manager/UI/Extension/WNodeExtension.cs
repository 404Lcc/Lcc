namespace LccHotfix
{
    public static class WNodeExtension
    {
        public static Window OpenChild(this WNode openBy, string windowName, object[] param = null)
        {
            return Entry.GetModule<WindowManager>().OpenWindow(openBy, windowName, param);
        }
    }
}