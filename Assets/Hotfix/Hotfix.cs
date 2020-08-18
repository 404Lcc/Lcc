using UnityEngine;

namespace Hotfix
{
    public class Hotfix : MonoBehaviour
    {
        public static void InitHotfix()
        {
            InitManager();
        }
        /// <summary>
        /// 初始化管理器
        /// </summary>
        private static void InitManager()
        {
            if (IO.manager == null)
            {
                GameObject original = new GameObject();
                original.name = "HotfixManager";
                original.tag = "HotfixManager";
                GameUtil.AddComponent<Manager>(original);
                DontDestroyOnLoad(original);
            }
        }
    }
}