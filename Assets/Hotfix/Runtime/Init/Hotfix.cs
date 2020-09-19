using UnityEngine;

namespace Hotfix
{
    public class Hotfix
    {
        public static void InitHotfix()
        {
            InitManager();
        }
        /// <summary>
        /// 初始化管理器
        /// </summary>
        public static void InitManager()
        {
            Manager.Instance.InitManager();
        }
    }
}