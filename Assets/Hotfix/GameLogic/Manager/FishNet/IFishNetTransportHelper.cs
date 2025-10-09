using UnityEngine;

namespace LccHotfix
{
    public interface IFishNetTransportHelper
    {
        /// <summary>
        /// 安装传输器
        /// </summary>
        /// <param name="networkManager"></param>
        void SetupTransport(GameObject networkManager);
    }
}