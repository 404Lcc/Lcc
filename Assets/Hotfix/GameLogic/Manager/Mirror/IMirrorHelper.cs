using UnityEngine;

namespace LccHotfix
{
    public interface IMirrorHelper
    {
        /// <summary>
        /// 安装
        /// </summary>
        MirrorNetworkManager Setup();

        void SetNetworkAddress(GameObject networkManager, string networkAddress);
    }
}