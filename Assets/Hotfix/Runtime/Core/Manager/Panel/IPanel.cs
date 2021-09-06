using UnityEngine;

namespace LccHotfix
{
    public interface IPanel
    {
        PanelType Type
        {
            get; set;
        }
        PanelState State
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
        void OpenPanel();
        void ClosePanel();
    }
}