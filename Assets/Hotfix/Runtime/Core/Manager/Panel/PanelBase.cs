using UnityEngine;

namespace Hotfix
{
    public class PanelBase
    {
        public PanelState state;
        public ObjectBase objectBase;
        public GameObject gameObject;
        public virtual void OpenPanel()
        {
        }
        public virtual void ClosePanel()
        {
        }
        public bool Exist()
        {
            if (gameObject == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}