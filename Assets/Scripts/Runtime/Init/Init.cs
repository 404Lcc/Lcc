using UnityEngine;

namespace LccModel
{
    public class Init : MonoBehaviour
    {
        void Start()
        {
            Manager.Instance.InitManager();
            EventManager.Instance.InitManager();
            UIEventManager.Instance.InitManager();

            EventManager.Instance.Publish(new Start());
        }
    }
}