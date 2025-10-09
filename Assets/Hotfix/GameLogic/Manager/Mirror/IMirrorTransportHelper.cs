using UnityEngine;

namespace LccHotfix
{
    public interface IMirrorTransportHelper
    {
        /// <summary>
        /// 安装传输器
        /// </summary>
        /// <param name="networkManager"></param>
        void SetupTransport(GameObject networkManager);
    }
}