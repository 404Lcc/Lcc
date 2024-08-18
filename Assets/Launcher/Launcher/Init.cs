using UnityEngine;

namespace LccModel
{
    public class Init : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            Launcher.Instance.Init();
        }
    }
}