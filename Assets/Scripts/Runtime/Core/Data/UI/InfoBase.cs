using UnityEngine;

namespace Model
{
    public class InfoBase
    {
        public InfoState state;
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