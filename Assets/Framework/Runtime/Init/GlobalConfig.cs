using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public enum HotfixMode
    {
        Mono = 1,
        ILRuntime = 2,
        HybridCLR = 3,
    }
    [CreateAssetMenu(menuName = "Lcc/CreateGlobalConfig", fileName = "GlobalConfig", order = 0)]
    public class GlobalConfig : ScriptableObject
    {
        public EPlayMode playMode;
        public string hostServer;
        public string version;

        public HotfixMode hotfixMode;
        public bool isRelease;
        public List<string> aotMetaAssemblyNameList;
        public string hotfix;
    }
}