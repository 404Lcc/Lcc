using UnityEngine;

namespace LccModel
{
    public enum HotfixMode
    {
        Mono = 1,
        ILRuntime = 2,
    }
    [CreateAssetMenu(menuName = "Lcc/CreateGlobalConfig", fileName = "GlobalConfig", order = 0)]
    public class GlobalConfig : ScriptableObject
    {
        public HotfixMode hotfixMode;
        public bool isRelease;
    }
}