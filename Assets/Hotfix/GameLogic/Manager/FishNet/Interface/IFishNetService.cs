using System;
using FishNet.Connection;

namespace LccHotfix
{
    public interface IFishNetService : IService
    {
        void Init();

        /// <summary>
        /// 设置传输器
        /// </summary>
        /// <param name="transportHelper"></param>
        void SetTransportHelper(IFishNetTransportHelper transportHelper);

        /// <summary>
        /// 设置IP
        /// </summary>
        /// <param name="networkAddress"></param>
        public void SetNetworkAddress(string networkAddress = "");

        /// <summary>
        /// 设置服务器消息分发器
        /// </summary>
        /// <param name="messageDispatcherHelper"></param>
        public void SetServerMessageDispatcherHelper(IFishNetServerMessageDispatcherHelper messageDispatcherHelper);

        /// <summary>
        /// 设置客户端消息分发器
        /// </summary>
        /// <param name="messageDispatcherHelper"></param>
        public void SetClientMessageDispatcherHelper(IMessageDispatcherHelper messageDispatcherHelper);

        /// <summary>
        /// 设置回调
        /// </summary>
        /// <param name="callbackHelper"></param>
        public void SetCallbackHelper(IMirrorCallbackHelper callbackHelper);

        /// <summary>
        /// 判断网络是否连接
        /// </summary>
        /// <returns></returns>
        public bool IsNetworkActive();

        /// <summary>
        /// 开启主机
        /// </summary>
        public void StartHost();

        /// <summary>
        /// 停止主机
        /// </summary>
        public void StopHost();

        /// <summary>
        /// 客户端开始连接
        /// </summary>
        public void Connect();

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void Disconnect();

        #region 客户端接口

        /// <summary>
        /// 客户端注册消息
        /// </summary>
        public void ClientRegisterMessage(int code, Action<MessageObject> handle);

        /// <summary>
        /// 客户端解绑消息
        /// </summary>
        public void ClientUnregisterMessage(int code, Action<MessageObject> handle);

        /// <summary>
        /// 客户端发送消息
        /// </summary>
        public void ClientSendMessage<T>(T message) where T : MessageObject;

        #endregion

        #region 服务器接口

        /// <summary>
        /// 服务器注册消息
        /// </summary>
        public void ServerRegisterMessage(int code, Action<NetworkConnection, MessageObject> handle);

        /// <summary>
        /// 服务器解绑消息
        /// </summary>
        public void ServerUnregisterMessage(int code, Action<NetworkConnection, MessageObject> handle);

        /// <summary>
        /// 服务器发送消息
        /// </summary>
        public void ServerSendMessage<T>(NetworkConnection client, T message) where T : MessageObject;

        /// <summary>
        /// 服务器广播消息
        /// </summary>
        public void ServerBroadcastMessage<T>(T message, NetworkConnection exclusionClient = null) where T : MessageObject;

        #endregion
    }
}