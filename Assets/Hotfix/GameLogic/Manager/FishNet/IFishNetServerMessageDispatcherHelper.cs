using System;
using FishNet.Connection;

namespace LccHotfix
{
    public interface IFishNetServerMessageDispatcherHelper
    {
        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="handle"></param>
        public void RegisterMessage(int code, Action<NetworkConnection, MessageObject> handle);

        /// <summary>
        /// 解绑消息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="handle"></param>
        public void UnregisterMessage(int code, Action<NetworkConnection, MessageObject> handle);

        /// <summary>
        /// 分发消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void DispatcherMessage(NetworkConnection client, NetworkMessage message);
    }
}