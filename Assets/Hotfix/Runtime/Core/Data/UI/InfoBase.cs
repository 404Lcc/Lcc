using UnityEngine;

namespace Hotfix
{
    public class InfoBase
    {
        public InfoState state;
        public ObjectBase objectBase;
        public GameObject container;
        public virtual void OpenPanel()
        {
        }
        public virtual void ClosePanel()
        {
        }
        public bool ContainerExist()
        {
            if (container == null)
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