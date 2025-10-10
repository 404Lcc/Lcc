using UnityEngine;

namespace LccHotfix
{
    public interface IFishNetHelper
    {
        /// <summary>
        /// 安装
        /// </summary>
        FishNet.Managing.NetworkManager Setup();

        void SetNetworkAddress(GameObject networkManager, string networkAddress);
    }
}