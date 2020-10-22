namespace LccHotfix
{
    public static class ItemTypeExpand
    {
        public static string ToItemString(this ItemType type)
        {
            return type.ToString() + "Item";
        }
    }
}