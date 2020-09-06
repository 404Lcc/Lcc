using UnityEngine;

namespace Hotfix
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameUtil.GetComponent<T>(Objects.manager.gameObject);
                }
                return _instance;
            }
        }
    }
}