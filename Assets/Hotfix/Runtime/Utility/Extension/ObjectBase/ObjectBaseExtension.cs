namespace LccHotfix
{
    public static class ObjectBaseExtension
    {
        public static void SafeDestroy(this AObjectBase aObjectBase)
        {
            if (aObjectBase == null) return;
            aObjectBase.Dispose();
        }
    }
}