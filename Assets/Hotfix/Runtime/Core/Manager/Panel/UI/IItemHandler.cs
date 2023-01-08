using UnityEngine;

namespace LccHotfix
{
    public interface IItemData
    {
    }
    public interface IItemHandler
    {
        void OnInitComponent(GameObject item);
        void UpdateData(IItemData data);
    }
}