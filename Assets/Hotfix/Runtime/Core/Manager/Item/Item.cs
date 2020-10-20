namespace Hotfix
{
    public class Item : IItem
    {
        public ItemType Type
        {
            get; set;
        }
        public AObjectBase AObjectBase
        {
            get; set;
        }
    }
}