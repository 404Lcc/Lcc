using System;
using UnityEngine;

namespace LccHotfix
{
    public interface IMirrorHelper
    {
        /// <summary>
        /// 安装
        /// </summary>
        void Setup(AssetLoader loader, Action<MirrorNetworkManager> onFinished);

        void SetNetworkAddress(GameObject networkManager, string networkAddress);
    }
}