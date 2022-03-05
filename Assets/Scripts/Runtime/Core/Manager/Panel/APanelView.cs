using UnityEngine;

namespace LccModel
{
    public abstract class APanelView<T> : AViewBase<T> where T : ViewModelBase
    {
        public GameObject gameObject => GetParent<GameObjectEntity>().gameObject;
        public override void Start()
        {
            AutoReference(gameObject);
            ShowView(gameObject);
        }
        public virtual void ClosePanel()
        {
            PanelType type = GetType().Name.ToPanelType();
            PanelManager.Instance.ClosePanel(type);
        }
        public virtual void ClearPanel()
        {
            PanelType type = GetType().Name.ToPanelType();
            PanelManager.Instance.ClearPanel(type);
        }
    }
}