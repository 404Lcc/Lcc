using UnityEngine;

namespace Hotfix
{
    public class Panel : PanelBase
    {
        public PanelType type;
        public override void OpenPanel()
        {
            if (ContainerExist())
            {
                state = PanelState.Open;
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
                state = PanelState.Close;
                container.SetActive(false);
            }
            else
            {
                Debug.Log(type.ToString() + ":null");
            }
        }
    }
}