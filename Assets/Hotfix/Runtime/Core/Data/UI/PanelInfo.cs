using UnityEngine;

namespace Hotfix
{
    public class PanelInfo : InfoBase
    {
        public PanelType type;
        public override void OpenPanel()
        {
            if (ContainerExist())
            {
                state = InfoState.Open;
                container.SetActive(true);
            }
            else
            {
                Debug.Log(type.ToString() + ":null");
            }
        }
        public override void ClosePanel()
        {
            if (ContainerExist())
            {
                state = InfoState.Close;
                container.SetActive(false);
            }
            else
            {
                Debug.Log(type.ToString() + ":null");
            }
        }
    }
}