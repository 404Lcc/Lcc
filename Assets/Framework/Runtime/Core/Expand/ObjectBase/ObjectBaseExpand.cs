namespace LccModel
{
    public static class ObjectBaseExpand
    {
        public static void SafeDestroy(this AObjectBase aObjectBase)
        {
            if (aObjectBase == null) return;
            aObjectBase.Dispose();
        }
    }
}