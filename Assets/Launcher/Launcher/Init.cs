using UnityEngine;

namespace LccModel
{
    public class Init : MonoBehaviour
    {
        void Start()
        {
#if URP
            
#else
            GameObject.Find("Global/MainCamera").GetComponent<Camera>().depth = 0;
            GameObject.Find("Global/UI Root/UICamera").GetComponent<Camera>().depth = 10;
            GameObject.Find("Global/AdaptCamera").GetComponent<Camera>().depth = 20;
#endif
            DontDestroyOnLoad(this.gameObject);
            Launcher.Instance.Init();
        }
    }
}