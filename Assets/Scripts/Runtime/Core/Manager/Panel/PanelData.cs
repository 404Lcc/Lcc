using UnityEngine;

namespace Model
{
    public class PanelData : PanelBase
    {
        public PanelType type;
        public override void OpenPanel()
        {
            if (Exist())
            {
                state = PanelState.Open;
                gameObject.SetActive(true);
            }
            else
            {
                Debug.Log(type.ToString() + ":null");
            }
        }
        public override void ClosePanel()
        {
            if (Exist())
            {
                state = PanelState.Close;
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log(type.ToString() + ":null");
            }
        }
    }
}