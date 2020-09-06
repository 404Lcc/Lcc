using UnityEngine;

namespace Hotfix
{
    public class Objects
    {
        private static Manager _manager;
        public static Manager manager
        {
            get
            {
                if (_manager == null)
                {
                    GameObject manager = GameUtil.GetGameObjectConvertedToTag("HotfixManager");
                    if (manager == null) return null;
                    _manager = GameUtil.GetComponent<Manager>(manager);
                }
                return _manager;
            }
        }
    }
}