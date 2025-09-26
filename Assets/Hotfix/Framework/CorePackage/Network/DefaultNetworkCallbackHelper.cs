using UnityEngine;

namespace LccHotfix
{
    public class DefaultNetworkCallbackHelper : INetworkCallbackHelper
    {
        public void OnConnectedCallback()
        {
            Debug.Log("客户端连接成功");
        }

        public void OnDisconnectedCallback()
        {
            Debug.Log("客户端断开连接");
        }
    }
}