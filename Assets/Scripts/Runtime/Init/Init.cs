using UnityEngine;

namespace Model
{
    public class Init : MonoBehaviour
    {
        void Start()
        {
            EventManager.Instance.InitManager();
            UIEventManager.Instance.InitManager();

            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, false, AssetType.UI));
            //TipsManager.Instance.InitManager(new TipsPool(10));
            //TipsWindowManager.Instance.InitManager(new TipsWindowPool(10));

            EventManager.Instance.Publish(new Start());
        }
    }
}