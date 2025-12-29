using System;
using UnityEngine;

namespace LccHotfix
{
    public interface IFishNetHelper
    {
        /// <summary>
        /// 安装
        /// </summary>
        void Setup(AssetLoader loader, Action<FishNet.Managing.NetworkManager> onFinished);

        void SetNetworkAddress(GameObject networkManager, string networkAddress);
    }
}