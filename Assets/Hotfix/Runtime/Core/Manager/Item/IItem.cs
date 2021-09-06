using UnityEngine;

namespace LccHotfix
{
    public interface IItem
    {
        ItemType Type
        {
            get; set;
        }
        AObjectBase AObjectBase
        {
            get; set;
        }
        GameObject gameObject
        {
            get;
        }
    }
}