namespace LccHotfix
{
    public static class IUILogicExtension
    {
        public static Window OpenChild(this IUILogic logic, string windowName, object[] param = null)
        {
            return Entry.GetModule<WindowManager>().OpenWindow(logic.WNode, windowName, param);
        }
    }
}