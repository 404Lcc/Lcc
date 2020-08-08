using UnityEngine;

namespace Hotfix
{
    public class Hotfix : MonoBehaviour
    {
        public static void InitHotfix()
        {
            InitGManager();
        }
        /// <summary>
        /// 初始化游戏管理器
        /// </summary>
        private static void InitGManager()
        {
            if (IO.gManager == null)
            {
                GameObject original = new GameObject();
                original.name = "GManager";
                original.tag = "HotfixManager";
                GameUtil.AddComponent<GManager>(original);
                DontDestroyOnLoad(original);
            }
        }
    }
}