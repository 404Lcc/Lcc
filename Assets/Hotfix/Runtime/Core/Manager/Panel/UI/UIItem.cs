using UnityEngine;

namespace LccHotfix
{
    public abstract class UIItem : UIComponent, IItemHandler
    {
        public virtual void OnInitComponent(GameObject item)
        {
            InitComponent(item);
        }
        public virtual void UpdateData(IItemData data)
        {
        }
    }
}